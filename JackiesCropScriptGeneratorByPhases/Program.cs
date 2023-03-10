using System;
using System.IO;
using System.Collections.Generic;
namespace SibSmileCropScriptGeneratorByPhases
{
    
    public class PhaseColumn
    {
        public String szPhaseName = "";
        public int iPos = 1;
        
        public PhaseColumn(String name, int pos)
        {
            szPhaseName = name;
            iPos = pos;
        }
    }
    
    public class Phase
    {
        public String szName = "";
        public double startSec = 0;
        public double duration = 0;
        public Dictionary<String, int> aliceTypeCount = new Dictionary<string, int>();
        public Phase ( String name, double start, double dur)
        {
            szName = name;
            startSec = start;
            duration = dur;
        }

    }

    public class SibSmile
    {
        public List<Phase> phases = new List<Phase>();
        public String szAudioFileName = "";
        public String szAudioFileName16 = "";
        

    }
    public class Program
    {
        public static Dictionary<String, int> phaseDecPosCols = new Dictionary<string, int>(); 
        public static void getSibSmilePhasesClips(Dictionary<String, SibSmile> sibSmiles, String szAliceOutputDir, String shOutput)
        {
            string[] files = Directory.GetFiles(szAliceOutputDir);
            TextWriter sw = new StreamWriter(shOutput, false);
            int iDebug = 0;
            foreach (string szFileName in files)
            {
                using (StreamReader sr = new StreamReader(szFileName))
                {
                    
                    if (!sr.EndOfStream)
                    {
                        try
                        {
                            while (!sr.EndOfStream)
                            {

                                String commaLine = sr.ReadLine();
                                String[] line = commaLine.Split(' ');
                                iDebug++;
                                if (iDebug == 9)
                                {
                                    bool debug = true;
                                }
                                if (line.Length >= 7)
                                {
                                    ////SPEAKER SN006_SSP_VTS_01_1_16000_mono_16bit 1 0.010 1.390 <NA> <NA> FEM <NA> <NA>
                                    String szFileName16 = line[1].Trim() + ".wav";
                                    if (!sibSmiles.ContainsKey(szFileName16))
                                    {
                                        szFileName16 = szFileName16 = szFileName16.Replace("_16", "_16000_mono_16bit"); //000_mono_16bit

                                    }
                                        String type = line[7].Trim();

                                    double startAlice = Convert.ToDouble(line[3].Trim());
                                    double endAlice = startAlice + (Convert.ToDouble(line[4].Trim()));

                                    if (sibSmiles.ContainsKey(szFileName16))
                                    {
                                        //foreach (SibSmile sb in sibSmiles.Values)
                                        {
                                            foreach (Phase ph in sibSmiles[szFileName16].phases)
                                            {
                                                double phaseStart = ph.startSec;
                                                double phaseEnd = ph.startSec + ph.duration;
                                                if ((startAlice >= phaseStart && startAlice <= phaseEnd) ||
                                                    (endAlice >= phaseStart && endAlice <= phaseEnd))
                                                {
                                                    if (!ph.aliceTypeCount.ContainsKey(type))
                                                        ph.aliceTypeCount.Add(type, 1);
                                                    else
                                                        ph.aliceTypeCount[type] = ph.aliceTypeCount[type] + 1;

                                                    sw.WriteLine("ffmpeg -i " + szFileName16.Replace("_16000_mono_16bit", "") + " -ss " + startAlice + " -to " + endAlice + " -c copy PHASES_CLIPS/" + sibSmiles[szFileName16].szAudioFileName.Replace(".wav", (ph.szName + "_" + type + "_" + ph.aliceTypeCount[type] + ".wav;")));
                                                }
                                            }


                                        }
                                    }
                                    else
                                    {
                                        bool debug = true;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {

                        }



                    }
                }

            

            }
            sw.Close();
        }

        public static Dictionary<String, SibSmile> getSibSmilePhases(String szFileName, int iPhaseColFrom, int iFileName, int iFileName16)
        {
            Dictionary<String, SibSmile> sibSmilePhases = new Dictionary<string, SibSmile>();

            if (File.Exists(szFileName))
            {
                
                using (StreamReader sr = new StreamReader(szFileName))
                {
                    if (!sr.EndOfStream)
                    {
                        String commaLine = sr.ReadLine();
                        String[] line = commaLine.Split(',');

                        if(line.Length>=iPhaseColFrom+1)
                        {
                            for(int p= iPhaseColFrom;p<line.Length; p=p+2)
                            {
                                if(line[p].Trim()!="")
                                {
                                    phaseDecPosCols.Add(line[p].Trim(), p);
                                }

                            }

                        }

                    }
                    
                    if (!sr.EndOfStream)
                    {
                        String commaLine = sr.ReadLine();
                        String[] line = commaLine.Split(',');

                        while (!sr.EndOfStream)
                        {
                            commaLine = sr.ReadLine();
                            line = commaLine.Split(',');
                            if (line.Length >= iFileName  &&
                                line.Length >= iFileName16 &&
                                line[iFileName].Trim()!="" &&
                                line[iFileName16].Trim()!="")
                            {
                                SibSmile sibSmile = new SibSmile();
                                sibSmile.szAudioFileName = line[iFileName].Trim();
                                sibSmile.szAudioFileName16 = line[iFileName16].Trim();

                                foreach (String phaseName in phaseDecPosCols.Keys)
                                {
                                    if (line.Length >= phaseDecPosCols[phaseName]+1)
                                    {
                                        Phase phase = new Phase(phaseName, Convert.ToDouble(line[phaseDecPosCols[phaseName]]), Convert.ToDouble(line[phaseDecPosCols[phaseName] + 1]));
                                        sibSmile.phases.Add(phase);
                                    }
                                }

                                sibSmilePhases.Add(sibSmile.szAudioFileName16, sibSmile);
                            }
                             
                        }

                    }
                }
            }

            return sibSmilePhases;
        }
    
        
        public static void Main(string[] args)
        {

            //SPEAKER SN006_SSP_VTS_01_1_16000_mono_16bit 1 0.010 1.390 <NA> <NA> FEM <NA> <NA>
            // Dictionary<String, Object >

            //D:\ALICE\SCRIPTSANDCLIPS\Sibs_SSP_VideoTimes_forALICE_02.02.2023_start_dur.csv
            Dictionary<String, SibSmile> sibSmilePhases = getSibSmilePhases("D:\\ALICE\\SCRIPTSANDCLIPS\\Sibs_SSP_VideoTimes_forALICE_02.16.2023_start_dur.csv", 18, 1,0);// Sibs_SSP_VideoTimes_forALICE_02.02.2023_start_dur.csv", 18, 1,0);
            getSibSmilePhasesClips(sibSmilePhases, "D:\\ALICE\\SCRIPTSANDCLIPS\\ALICEOUTPUTS", "D:\\ALICE\\SCRIPTSANDCLIPS\\SHSCRIPTS"+DateTime.Now.Month+"_" + DateTime.Now.Day + "_" + DateTime.Now.Year+"_"+ new Random().Next()+ ".sh");

            Console.Read();
            Console.Read();
        }
    }
}
