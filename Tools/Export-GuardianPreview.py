"""Encode the native -moonlitGuardianQA -guardianFilm frames and recorded sound cues."""
import argparse
import csv
from pathlib import Path
import subprocess
import wave

import numpy as np


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("capture", type=Path, help="QA/Guardian directory produced by the Windows player")
    parser.add_argument("output", type=Path)
    parser.add_argument("--ffmpeg", default="ffmpeg")
    args = parser.parse_args()
    count = int((args.capture / "frame-count.txt").read_text(encoding="utf-8-sig"))
    frames = [args.capture / "Frames" / f"{i:04d}.png" for i in range(count)]
    if not frames or any(not frame.is_file() for frame in frames):
        raise ValueError("Incomplete native frame sequence")
    rate = 32000
    bank = Path(__file__).resolve().parents[1] / "Assets/Moonlit/Resources/MoonAudio"
    duration = len(frames) / 30
    output = np.zeros((round(rate * duration), 2), dtype=np.float32)
    def decode(path):
        raw = subprocess.check_output([args.ffmpeg, "-loglevel", "error", "-i", str(path),
                                       "-f", "f32le", "-ac", "2", "-ar", str(rate), "pipe:1"])
        return np.frombuffer(raw, dtype="<f4").reshape(-1, 2)
    theme = decode(bank / "mus_guardian.ogg")
    for start in range(0, len(output), len(theme)):
        count = min(len(theme), len(output) - start)
        output[start:start + count] = theme[:count] * .35
    sounds = {}
    with (args.capture / "sound-events.csv").open(encoding="utf-8-sig", newline="") as src:
        for seconds, name in csv.reader(src):
            if name not in sounds:
                filename = f"jingle_{name}.ogg" if name in {"win", "lose"} else f"sfx_{name}.wav"
                sounds[name] = decode(bank / filename)
            event = sounds[name]
            start = round(float(seconds) * rate)
            count = min(len(event), len(output) - start)
            if count > 0:
                output[start:start + count] += event[:count] * .53
    fade = min(round(rate * .24), len(output))
    output[-fade:] *= np.linspace(1, 0, fade)[:, None]
    peak = np.max(np.abs(output))
    output *= min(1, .93 / max(peak, .001))
    track = args.capture / "mix.wav"
    with wave.open(str(track), "wb") as dest:
        dest.setparams((2, 2, rate, 0, "NONE", ""))
        dest.writeframes(np.rint(output * 32767).astype("<i2").tobytes())
    args.output.parent.mkdir(parents=True, exist_ok=True)
    subprocess.run([args.ffmpeg, "-y", "-hide_banner", "-loglevel", "error", "-framerate", "30",
                    "-i", str(args.capture / "Frames/%04d.png"), "-i", str(track),
                    "-frames:v", str(len(frames)), "-c:v", "libx264", "-preset", "fast", "-crf", "18",
                    "-pix_fmt", "yuv420p", "-c:a", "aac", "-b:a", "144k", "-movflags", "+faststart",
                    str(args.output)], check=True)
    print(f"{len(frames)} native frames, {duration:.2f}s, {len(sounds)} kinds of sound")


if __name__ == "__main__":
    main()
