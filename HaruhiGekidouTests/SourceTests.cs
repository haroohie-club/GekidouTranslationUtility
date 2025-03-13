using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using HaruhiGekidouLib;
using HaruhiGekidouLib.Archive;
using HaruhiGekidouLib.Util;
using HaruhiGekidouLib.Script;


namespace HaruhiGekidouTests.Tests
{
    public class SourceTests
    {

        private static readonly string[] _scriptArcNames =      
        [
            //"Bonus_000",  i don't think Bonus contains script data, so I'll pass on them for now
            //"Bonus_001",
            //"Bonus_002",
            //"Bonus_003",
            //"Bonus_004",
            //"Bonus_005",
            "Scene_000",
            "Scene_001",
            "Scene_002",
            "Scene_003",
            "Scene_004",
            "Scene_005",
            "Scene_006",
            "Scene_007",
            "Scene_008",
            "Scene_009",
            "Scene_020",
            "Scene_021",
            "Scene_022",
            "Scene_040",
            "Tutorial_000",
            "Tutorial_001",
            "Tutorial_002",
            "Tutorial_003",
            "Tutorial_004",
            "Tutorial_005",
            "Tutorial_006"
        ];

        public static string[] _AdvPartScriptFiles()
        {
            if (Directory.Exists("./input/AdvPartScript/"))
            {
                return Directory.GetFiles("./input/AdvPartScript/");
            }
            else
            {
                Directory.CreateDirectory("./input/AdvPartScript/");

                List<string> newScripts = new List<string>();
                string [] arcs = Directory.GetFiles("./input/ScriptArc/");
                foreach (string arc in arcs)
                {
                    if (_scriptArcNames.Contains(Path.GetFileNameWithoutExtension(arc)))
                    {
                        byte[] arcBytes = File.ReadAllBytes(arc);
                        GekidouArc newArc = new(arcBytes);
                        
                        bool isInScript = false;
                        foreach (GekidouArcEntry entry in newArc.Entries)
                        {
                            if (entry.IsDirectory)
                            {
                                isInScript = entry.Name == "AdvPartScript";
                            }
                            else
                            {
                                if (!isInScript) continue;
                                string name = Path.GetFileNameWithoutExtension(entry.Name);
                                if (arc.Contains("Tutorial"))
                                {
                                    name += arc.Split("_")[1];
                                    
                                }
                                File.WriteAllBytes(Path.Combine("./input/AdvPartScript/", name + ".bin"), entry.Data);
                                newScripts.Add(Path.Combine("./input/AdvPartScript/", name + ".bin"));
                            }
                        }

                    }
                }
                return newScripts.ToArray();
            }
        }


        [Test]
        [TestCaseSource(nameof(_scriptArcNames))]
        [Parallelizable(ParallelScope.All)]
        public async Task ScriptArcTest(string _arcFileName)
        {
            //find the arc
            //load the arc
            byte[] arcBytes = File.ReadAllBytes("./input/ScriptArc/" + _arcFileName + ".arc");
            
            //create a new arc
            GekidouArc newArc = new(arcBytes);
            byte[] newBytes = newArc.GetBytes();

            //save them out
            Directory.CreateDirectory("./output/ScriptArc/");
            File.WriteAllBytes("./output/ScriptArc/" + _arcFileName + ".arc", newBytes);
            
            //compare the two
            Assert.That(newBytes, Is.EqualTo(arcBytes));

        }
        
        

        [Test]
        [TestCaseSource(nameof(_AdvPartScriptFiles))]
        [Parallelizable(ParallelScope.All)]
        public void validateScript(string scriptPath)
        {
            
            byte[] scriptBytes = File.ReadAllBytes(scriptPath);

            AdvPartScript newScript = new("TestScript", scriptBytes);
            byte[] newScriptBytes = newScript.GetBytes();
            
            //save them out
            Directory.CreateDirectory("./output/AdvPartScript/");
            File.WriteAllBytes("./output/AdvPartScript/" + Path.GetFileName(scriptPath), newScriptBytes);
            //compare
            Assert.That(newScriptBytes, Is.EqualTo(scriptBytes));
        }
    }
}
