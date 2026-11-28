"""Regenerate the original Cuội foley WAVs with NumPy. FestivalPiano.py renders the music."""
from pathlib import Path
import argparse
import uuid
import wave

import numpy as np


ROOT = Path(__file__).resolve().parents[1] / "Assets/Moonlit/Resources"
DEST = ROOT / "MoonAudio"
RATE = 24000
TAU = 2 * np.pi
rng = np.random.default_rng(20261124)
# Preserve the exact foley noise sequence used by the shipped production recordings.
rng.bit_generator.state = {
    "bit_generator": "PCG64",
    "state": {"state": 62098568395393058365064810221115598609,
              "inc": 279070807638389550802058384858475990131},
    "has_uint32": 0, "uinteger": 0,
}
wildlife_rng = np.random.default_rng(20261124)


def frequency(midi):
    return 440 * 2 ** ((midi - 69) / 12)


def voice(kind, hz, length, end=None, source=None):
    t = np.arange(round(RATE * length)) / RATE
    p = t / length
    env = np.minimum(1, t / .007) * np.minimum(1, (length - t) / .03)
    bend = hz if end is None else hz + (end - hz) * p
    if kind == "reed":
        bend = bend * (1 + .004 * np.sin(TAU * 5 * t) * np.minimum(1, t * 4))
    phase = TAU * np.cumsum(np.full(t.shape, bend)) / RATE
    oscillator = np.sin(phase)
    noise = (rng if source is None else source).normal(0, .3, len(t))
    if kind == "reed":
        tone = oscillator + .26 * np.sin(2 * phase) + .07 * np.sin(3 * phase) + noise * .12
        env *= np.minimum(1, t / .11) * np.minimum(1, (length - t) / .13)
    elif kind == "string":
        tone = oscillator + .45 * np.sin(2 * phase) * np.exp(-t * 9) + .23 * np.sin(4.02 * phase)
        env *= np.exp(-4.5 * p)
    elif kind == "bell":
        tone = oscillator + .34 * np.sin(2.38 * phase) * np.exp(-t * 5) + .18 * np.sin(3.01 * phase)
        env *= np.exp(-4.3 * p)
    elif kind == "drum":
        tone = oscillator + .40 * np.sin(1.51 * phase) + noise * .48
        env *= np.exp(-7 * p)
    elif kind == "rustle":
        tone = noise * 2 + .20 * oscillator
        env *= np.sin(np.pi * p) ** .65
    elif kind == "bark":
        tone = np.tanh(3 * oscillator + 1.4 * np.sin(2 * phase)) + noise * .55
        env *= np.exp(-3 * p)
    elif kind == "caw":
        throat = phase + 1.4 * np.sin(TAU * 31 * t)
        tone = .35 * np.sin(throat) + .46 * np.sin(3 * throat) + .32 * np.sin(5 * throat) + noise * .29
        env *= np.sin(np.pi * p) ** .66 * (1 + .20 * np.sin(TAU * 23 * t))
    elif kind == "water":
        hiss = noise - np.roll(noise, 1) * .73
        tone = .76 * hiss + .48 * oscillator * np.exp(-p * 7)
        env *= np.exp(-p * 5)
    else:
        tone = oscillator + .18 * np.sin(2 * phase)
        env *= np.exp(-2.5 * p)
    return env * tone


effects = {
    # (seconds from onset, instrument, start Hz, end Hz, length, gain)
    "jump": [(0,"string",250,480,.16,.7)],
    "airJump": [(0,"reed",450,730,.16,.75),(.07,"bell",780,900,.20,.4)],
    "dash": [(0,"rustle",730,150,.22,.8),(.04,"sine",440,188,.16,.42)],
    "climb": [(0,"drum",290,120,.08,.38),(.045,"rustle",90,80,.08,.35)],
    "land": [(0,"drum",110,55,.16,.8)],
    "step": [(0,"rustle",120,60,.065,.38)], "woodStep": [(0,"drum",280,150,.08,.48)],
    "crate": [(0,"rustle",93,68,.23,.40),(.06,"string",68,75,.15,.25)],
    "pulse": [(0,"rustle",340,100,.29,.7),(.04,"bell",262,523,.57,.75),(.12,"bell",660,784,.46,.35)],
    "ready": [(0,"bell",880,880,.28,.48),(.08,"bell",1175,1175,.35,.25)],
    "collect": [(0,"bell",784,784,.19,.6),(.065,"bell",1046,1046,.26,.65)],
    "checkpoint": [(i*.115,"bell",frequency(n),frequency(n),.70,.55) for i,n in enumerate([62,69,74,78])],
    "hurt": [(0,"string",200,75,.24,.9),(.025,"drum",135,54,.25,.75)],
    "dog": [(0,"bark",184,93,.15,.70),(.19,"bark",149,80,.14,.50)],
    "cat": [(0,"rustle",710,1330,.39,.68),(.10,"sine",950,520,.19,.2)],
    "carpBubbles": [(0,"sine",430,830,.09,.36),(.13,"sine",590,1180,.085,.47),(.31,"sine",490,1060,.11,.56)],
    "carpLeap": [(0,"water",258,760,.20,.83),(.04,"rustle",430,980,.19,.34),(.11,"sine",920,1460,.08,.24)],
    "carpSplash": [(0,"water",305,95,.36,.98),(.035,"water",590,225,.23,.65),(.14,"sine",410,780,.08,.23)],
    "crowCall": [(0,"caw",346,206,.23,.66),(.30,"caw",316,176,.25,.56)],
    "crowFlight": [(0,"rustle",390,85,.16,.65),(.15,"rustle",330,105,.13,.46),(.28,"rustle",290,118,.12,.36)],
    "stun": [(0,"bell",780,1200,.24,.55),(.08,"bell",1045,1560,.29,.32)],
    "warn": [(0,"drum",140,90,.32,.60),(.21,"drum",190,100,.29,.55)],
    "sweep": [(0,"rustle",68,156,.43,.60),(.075,"drum",103,42,.42,.5)],
    "bloom": [(i*.14,"bell",frequency(n),frequency(n),.9,.55) for i,n in enumerate([67,71,74,79])],
    "signal1": [(i*.12,"bell",frequency(n),frequency(n),.68,.52) for i,n in enumerate([62,69])],
    "signal2": [(i*.12,"bell",frequency(n),frequency(n),.74,.53) for i,n in enumerate([66,71,74])],
    "signal3": [(i*.13,"bell",frequency(n),frequency(n),.88,.52) for i,n in enumerate([69,74,78,81])],
    "uiMove": [(0,"string",570,695,.065,.50)], "uiClick": [(0,"string",570,820,.10,.77)]
}


guardian_foley = {
    "rootCoil": [(0,"string",126,95,.32,.58),(.16,"rustle",180,430,.36,.52),(.31,"string",210,145,.28,.45)],
    "rootDrum": [(0,"drum",120,62,.16,.62),(.22,"drum",97,48,.29,.78),(.43,"rustle",180,320,.25,.40)],
    "rootRush": [(0,"rustle",170,680,.40,.78),(.11,"drum",135,51,.20,.52),(.27,"rustle",370,160,.24,.65)],
    "rootImpact": [(0,"drum",142,40,.44,1.0),(.02,"rustle",480,96,.24,.85),(.09,"string",83,48,.48,.72),(.24,"drum",94,44,.30,.45)]
}


def render_effect(parts, source=None):
    mono = np.zeros(round((max(t + length for t, _, _, _, length, _ in parts) + .01) * RATE))
    for offset, instrument, hz, end, length, gain in parts:
        sample = voice(instrument, hz, length, end, source)
        at = round(offset * RATE)
        mono[at:at + len(sample)] += sample * gain
    return np.tanh(mono * 1.2)


def render_scenery():
    """Loopable original field textures and two short mechanisms. No recorded samples."""
    source = np.random.default_rng(271128)
    mechanical = {
        "woodLatch": [(0,"drum",410,183,.075,.63),(.055,"drum",290,126,.12,.72),
                      (.064,"string",185,145,.18,.21)],
        "deckSettle": [(0,"drum",125,59,.19,.73),(.045,"rustle",245,125,.18,.32),
                       (.12,"drum",236,113,.105,.42)],
    }
    for name, parts in mechanical.items():
        write_clip("sfx_" + name, render_effect(parts, source))

    seconds = 12
    frames = RATE * seconds
    p = np.arange(frames) / frames
    hz = np.fft.rfftfreq(frames, 1 / RATE)

    def band(low, high):
        # Circular filtering keeps the last-to-first sample step continuous.
        shape = (1 - np.exp(-(hz / low) ** 2)) * np.exp(-(hz / high) ** 2)
        noise = np.fft.irfft(np.fft.rfft(source.standard_normal(frames)) * shape)
        return noise / np.std(noise)

    for name in ("amb_canopy", "amb_river"):
        channels = []
        shared = band(160, 1200)
        for side in (-1, 1):
            if name == "amb_canopy":
                gust = (.45 + .29*np.cos(TAU*p + side*.28) + .12*np.sin(TAU*3*p)) ** 2
                leaves = (.70*band(680, 3300) + .30*shared) * gust
                rasp = np.maximum(0, np.cos(TAU*5*p + side*.38)) ** 9
                sound = .14*leaves + .025*band(1900, 5200)*rasp
            else:
                wash = .7 + .17*np.sin(TAU*2*p + side*.5) + .09*np.sin(TAU*7*p)
                sound = .072*(.6*shared + .70*band(400, 2700))*wash
                # Individual droplets overlap the loop boundary using periodic elapsed time.
                for _ in range(18):
                    t = (p * seconds - source.uniform(0, seconds)) % seconds
                    tone = source.uniform(510, 1080)
                    sound += .09*np.minimum(1, t/.012)*np.exp(-t*18)*np.sin(TAU*(tone*t + 18*(1-np.exp(-t*13))))
            channels.append(sound)
        write_clip(name, np.column_stack(channels))


def write_clip(name, samples):
    path = DEST / (name + ".wav")
    with wave.open(str(path), "wb") as out:
        out.setparams((1 if samples.ndim == 1 else samples.shape[1], 2, RATE, 0, "NONE", ""))
        out.writeframes(np.rint(np.clip(samples, -1, 1) * 32767).astype("<i2").tobytes())
    meta(path)


def meta(path):
    guid = uuid.uuid5(uuid.NAMESPACE_URL, "moonlit/audio/" + path.name).hex
    header = f"fileFormatVersion: 2\nguid: {guid}\n"
    if path.is_dir():
        body = "folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
    else:
        body = """AudioImporter:
  externalObjects: {}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 24000
    compressionFormat: 1
    quality: 0.85
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {}
  forceToMono: 0
  normalize: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 0
"""
    Path(str(path) + ".meta").write_text(header + body + "  userData: \n  assetBundleName: \n  assetBundleVariant: \n", encoding="utf-8")


def main():
    DEST.mkdir(exist_ok=True)
    parser = argparse.ArgumentParser()
    parser.add_argument("--guardian", action="store_true", help="Render only the banyan encounter sounds")
    parser.add_argument("--scenery", action="store_true", help="Render only environmental loops and mechanism foley")
    args = parser.parse_args()
    if args.scenery:
        render_scenery()
        return
    if not args.guardian:
        meta(DEST)
    bank = guardian_foley if args.guardian else {**effects, **guardian_foley}
    root_rng = np.random.default_rng(20261125)
    for name, parts in bank.items():
        source = root_rng if name in guardian_foley else wildlife_rng if name.startswith(("carp", "crow")) else None
        clip = render_effect(parts, source)
        path = DEST / ("sfx_" + name + ".wav")
        with wave.open(str(path), "wb") as out:
            out.setparams((1, 2, RATE, 0, "NONE", ""))
            out.writeframes(np.rint(clip * 32767).astype("<i2").tobytes())
        meta(path)
    if not args.guardian:
        render_scenery()


if __name__ == "__main__":
    main()
