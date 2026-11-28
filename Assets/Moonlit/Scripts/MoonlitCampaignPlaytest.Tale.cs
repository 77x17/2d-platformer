using System.IO;
using UnityEngine;

namespace Moonlit
{
    public sealed partial class MoonlitCampaignPlaytest
    {
        bool recordTale;
        string filmDir;
        int shot;
        float previousStep;
        void ObserveTale()
        {
            if (!recordTale) return;
            if (!game.InStory) {
                if(filmDir!=null) {
                    File.WriteAllText(Path.Combine(filmDir,"frames.txt"),shot.ToString());
                    filmDir=null;Time.captureDeltaTime=previousStep;
                }
                return;
            }
            if(filmDir==null) {
                string reel = game.State==GameState.Prologue ? "Opening" : "Reunion";
                filmDir=Path.Combine(directory,reel);
                Directory.CreateDirectory(filmDir);shot=0;
                previousStep=Time.captureDeltaTime;Time.captureDeltaTime=1f/30;
            }
            MoonlitCapture.Save(Path.Combine(filmDir,(shot++).ToString("D4")+".png"),960,540);
        }
    }
}
