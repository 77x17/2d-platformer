"""Render the six Cuội piano arrangements and two stings. Python 3, NumPy, ffmpeg, libsndfile.

python Tools/FestivalPiano.py --samples /path/to/salamander/mp3 --ffmpeg /path/to/ffmpeg
The original recording, required pitches and score sources are in Documentation/FESTIVAL_PIANO.md.
"""
import argparse
import ctypes as ct
import ctypes.util
import os
import subprocess
from pathlib import Path

import numpy as np


RATE = 32000
OUT = Path(__file__).resolve().parents[1] / "Assets/Moonlit/Resources/MoonAudio"
SAMPLES = {12 * (octave + 1) + step: f"{name}{octave}.mp3"
           for octave in range(2, 6) for name, step in (("C", 0), ("Ds", 3), ("Fs", 6), ("A", 9))}
SAMPLES[84] = "C6.mp3"

# Durations are quarter beats. Each row is one bar of 2/4; -1 is silence.
# Printed pp.85–86, from the pickup up to the first instrumental response.
TRONG_QUAN = [
    [(-1, 1.5), (72, .5)],
    [(72, .5), (69, .25), (67, .25), (62, .5), (65, .5)],
    [(67, 1), (62, .75), (65, .25)],
    [(67, .5), (60, 1), (72, .5)],
    [(72, .5), (69, .25), (67, .25), (62, .75), (65, .25)],
    [(67, .5), (65, .5), (60, .5), (62, .25), (65, .25)],
    [(60, .5), (60, .5), (-1, .5), (72, .5)],
    [(62, .5), (72, 1), (69, .5)],
    [(69, .75), (67, .25), (65, .5), (67, .25), (69, .25)],
    [(62, .75), (60, .25), (62, .5), (57, .25), (60, .25)],
    [(62, .5), (62, .5), (-1, .5), (57, .25), (60, .25)],
    [(62, .5), (65, .5), (69, .25), (67, .25), (65, .25), (60, .25)],
]

# Vi Minh Huy, example 6, printed p.42, first staff and its landing note.
# The printed excerpt starts partway through a measure; the first bar pads the pickup.
DI_CAY = [
    [(-1, 1), (71, .5), (69, 1/6), (71, 1/6), (69, 1/6)],
    [(67, 1), (71, .5), (69, .25), (71, .25)],
    [(74, 1), (71, .5), (74, .5)],
    [(71, 1), (69, .5), (71, .25), (69, .25)],
    [(67, .5), (-1, .5), (71, .5), (74, .5)],
    [(71, 1), (69, .5), (71, .25), (69, .25)],
    # A project-composed response follows the folk phrase's landing G.
    [(67, 1.5), (62, .5)],
    [(64, .5), (67, .5), (69, .5), (67, .5)],
    [(71, .75), (69, .25), (67, .5), (64, .5)],
    [(62, .5), (67, 1), (-1, .5)],
    [(69, .5), (67, .5), (64, .5), (62, .5)],
    [(67, 1.5), (-1, .5)],
]

# Bass and inner voices belong to these arrangements. Open fifths leave
# space for the modal turn of the Vietnamese melody.
TRONG_HARMONY = [(36, 43, 50), (38, 45, 53), (43, 50, 57), (36, 43, 50),
                 (38, 45, 53), (41, 48, 55), (36, 43, 52), (38, 45, 53),
                 (41, 48, 55), (38, 45, 53), (36, 43, 50), (38, 45, 53)]
DI_HARMONY = [(43, 50, 57), (43, 50, 59), (47, 54, 62), (40, 47, 55),
              (43, 50, 59), (45, 52, 59), (43, 50, 59), (40, 47, 55),
              (45, 52, 59), (38, 45, 54), (38, 45, 57), (43, 50, 59)]

# Positions, chord voices, duration, velocity. Piano left hand, two beats per bar.
PATTERNS = {
    "home":     [(0, 0, 1.8, .48), (.58, 1, .98, .31), (1.08, 2, .82, .38)],
    "village":  [(0, 0, .73, .60), (.53, 1, .43, .34), (1, 2, .61, .52), (1.53, 1, .38, .32)],
    "bamboo":   [(0, 0, .42, .62), (.48, 1, .32, .39), (.77, 2, .22, .31),
                 (1.05, 1, .41, .54), (1.52, 2, .32, .42)],
    "river":    [(0, 0, 1.70, .42), (.37, 1, 1.24, .29), (.74, 2, .93, .34), (1.35, 1, .53, .29)],
    "hill":     [(0, 0, 1.35, .55), (.10, 1, 1.21, .34), (1.12, 2, .77, .43)],
    "guardian": [(0, 0, .52, .69), (.48, 1, .28, .38), (.72, 0, .24, .45),
                 (1, 2, .49, .59), (1.49, 1, .29, .35), (1.76, 2, .21, .39)],
}

# Reducing or raising the key, pace, pedal and register changes the character.
TRACKS = [
    ("home", DI_CAY, DI_HARMONY, 74, 0, .47),
    ("village", TRONG_QUAN, TRONG_HARMONY, 90, 2, .20),
    ("bamboo", TRONG_QUAN, TRONG_HARMONY, 105, 5, .13),
    ("river", DI_CAY, DI_HARMONY, 70, 2, .64),
    ("hill", DI_CAY, DI_HARMONY, 80, -2, .35),
    ("guardian", TRONG_QUAN, TRONG_HARMONY, 112, 0, .12),
]


def load_samples(folder, encoder):
    bank = {}
    for note, name in SAMPLES.items():
        raw = subprocess.check_output([encoder, "-v", "error", "-i", str(folder / name),
                                       "-f", "f32le", "-ac", "2", "-ar", str(RATE), "pipe:1"])
        sample = np.frombuffer(raw, "<f4").reshape(-1, 2)
        maxima = np.max(np.abs(sample), axis=1)
        onset = max(0, np.flatnonzero(maxima > .002)[0] - int(.003 * RATE))
        sample = sample[onset: onset + RATE * 7]
        bank[note] = sample * (.62 / np.quantile(np.max(abs(sample[:RATE // 2]), axis=1), .997))
    return bank


class Performance:
    def __init__(self, bank, duration, seed, loop):
        self.bank = bank
        self.audio = np.zeros((round(duration * RATE), 2), np.float32)
        self.random = np.random.default_rng(seed)
        self.loop = loop

    def play(self, at, midi, held, velocity, pedal):
        origin = min(self.bank, key=lambda key: abs(key - midi))
        source = self.bank[origin]
        release = .15 + .69 * pedal + .003 * max(0, 65 - midi)
        n = round((held + release * 2.25) * RATE)
        position = np.arange(n) * 2 ** ((midi - origin) / 12)
        end = np.minimum(position.astype(int), len(source) - 2)
        fraction = (position - end).clip(0, 1).astype(np.float32)
        sound = source[end] + (source[end + 1] - source[end]) * fraction[:, None]
        # A soft stroke loses hammer brightness; there remains an acoustic sample attack.
        tone = .08 + .27 * (1 - velocity)
        sound[1:] = (1 - tone) * sound[1:] + tone * sound[:-1]
        envelope = np.exp(-np.maximum(0, np.arange(n) / RATE - held) * 3.5 / release)
        sound *= (envelope * velocity ** 1.65 * self.random.uniform(.97, 1.03))[:, None]
        pan = np.clip((midi - 60) / 95, -.28, .28)
        sound *= np.sqrt([1 - pan, 1 + pan])
        start = max(0, round((at + self.random.uniform(-.006, .006)) * RATE))
        length = min(n, len(self.audio) - start)
        if length <= 0:
            return
        self.audio[start:start + length] += sound[:length]
        if self.loop and length < n:
            self.audio[:n-length] += sound[length:]

    def master(self):
        # Small stereo early reflections; already-recorded room tails remain in the note samples.
        dry = self.audio.copy()
        for seconds, gain in ((.039, .065), (.071, .046), (.109, .031), (.173, .019)):
            shift = round(seconds * RATE)
            reflection = np.roll(dry[:, ::-1], shift, axis=0)
            if not self.loop:
                reflection[:shift] = 0
            self.audio += gain * reflection
        rms = np.sqrt(np.mean(self.audio ** 2))
        gain = min(.154 / max(rms, .001), 1.02 / max(.01, np.max(abs(self.audio))))
        mix = np.tanh(gain * self.audio * 1.02) * 1.18
        return mix.astype("<f4")


def arrangement(bank, name, motif, harmony, bpm, transpose, pedal):
    beats = 2 * len(motif)
    beat = 60 / bpm
    performance = Performance(bank, beats * 2 * beat, 8268 + bpm, True)
    for passage in range(2):
        for bar, (melody, chord) in enumerate(zip(motif, harmony)):
            assert abs(sum(duration for _, duration in melody) - 2) < 1e-5
            first = (passage * beats + 2 * bar) * beat
            swell = .04 * np.sin(np.pi * bar / 6)
            for pos, key, duration, velocity in PATTERNS[name]:
                event = (first + pos * beat, chord[key] + transpose, duration * beat, velocity + swell, pedal)
                performance.play(*event)
                if passage and key == 2 and bar in (2, 5, 8, 11):
                    performance.play(event[0] + .018, event[1] + 5, duration * beat * .84, .29, pedal)
            pos = 0
            for pitch, length in melody:
                if pitch >= 0:
                    # Keep the folk tune in front. On the repeat the upper answers lift a register.
                    lift = 12 if passage and name in ("home", "river") and bar >= 6 else 0
                    timing = first + pos * beat
                    velocity = (.82 if pos < .01 else .76) + swell + (.025 if passage else 0)
                    if length <= .26:
                        velocity -= .13
                    performance.play(timing, pitch + transpose + lift, length * beat * .90, velocity, pedal)
                    if passage and (name in ("guardian", "hill") and length >= .75):
                        performance.play(timing + .014, pitch + transpose - 12, length * beat * .77, .39, pedal)
                pos += length
    return performance.master()


def jingle(bank, win):
    performance = Performance(bank, 4.3 if win else 3.4, 92 if win else 93, False)
    notes = [(0,67,.36),(.34,71,.22),(.53,69,.12),(.66,71,.18),(.87,74,.51),
             (1.33,71,.28),(1.61,74,.31),(1.95,79,1.25)] if win else [
             (0,67,.42),(.55,62,.55),(1.10,60,.32),(1.48,62,.92)]
    for index, (t, note, hold) in enumerate(notes):
        performance.play(t, note, hold, .74 if win else .62, .48)
        if index in (0, len(notes)-1):
            performance.play(t + .015, (43 if win else 38), hold + .2, .48, .44)
            performance.play(t + .046, (50 if win else 45), hold + .13, .38, .44)
    return performance.master()


class SoundInfo(ct.Structure):
    _fields_ = [("frames", ct.c_int64), ("samplerate", ct.c_int), ("channels", ct.c_int),
                ("format", ct.c_int), ("sections", ct.c_int), ("seekable", ct.c_int)]


def save(library, output, audio):
    info = SoundInfo(0, RATE, 2, 0x200060, 0, 0)
    handle = library.sf_open(os.fsencode(output), 0x20, ct.byref(info))
    if not handle:
        raise OSError(f"Could not open {output}")
    try:
        library.sf_set_string(handle, 4, b"Cuoi Tim Trang; piano samples: Alexander Holm (CC BY 3.0)")
        data = np.ascontiguousarray(audio)
        for first in range(0, len(data), 2048):
            block = data[first:first + 2048]
            ptr = block.ctypes.data_as(ct.POINTER(ct.c_float))
            if library.sf_writef_float(handle, ptr, len(block)) != len(block):
                raise OSError(f"Short write for {output}")
    finally:
        library.sf_close(handle)
    print(f"{output.name}: {len(audio) / RATE:.2f}s, RMS {20*np.log10(np.sqrt(np.mean(audio**2))):.1f} dBFS")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--samples", required=True, type=Path)
    parser.add_argument("--ffmpeg", default="ffmpeg")
    parser.add_argument("--sndfile", default=os.getenv("SNDFILE_LIBRARY") or ctypes.util.find_library("sndfile"))
    args = parser.parse_args()
    if not args.sndfile:
        parser.error("Set --sndfile to your local libsndfile shared library.")
    if os.name == "nt" and Path(args.sndfile).is_absolute():
        dll_dir = os.add_dll_directory(str(Path(args.sndfile).parent))
    library = ct.CDLL(args.sndfile)
    library.sf_open.argtypes, library.sf_open.restype = [ct.c_char_p, ct.c_int, ct.POINTER(SoundInfo)], ct.c_void_p
    library.sf_set_string.argtypes = [ct.c_void_p, ct.c_int, ct.c_char_p]
    library.sf_writef_float.argtypes = [ct.c_void_p, ct.POINTER(ct.c_float), ct.c_int64]
    library.sf_writef_float.restype = ct.c_int64
    library.sf_close.argtypes = [ct.c_void_p]
    samples = load_samples(args.samples, args.ffmpeg)
    for configuration in TRACKS:
        save(library, OUT / f"mus_{configuration[0]}.ogg", arrangement(samples, *configuration))
    for ending in ("win", "lose"):
        save(library, OUT / f"jingle_{ending}.ogg", jingle(samples, ending == "win"))
