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
            ClassicAssert.AreEqual(arcBytes, newBytes);

        }
        
        

//        testing AdvScript files - unfinished right now
//        [Test]
//        [Parallelizable(ParallelScope.All)]
//        public void validateScript(byte[] script)
//        {
//            //create new script based on input
//            AdvPartScript Newscript = new("TestScript", script);
//            byte[] newScriptBytes = Newscript.GetBytes();
//
//            //compare
//           ClassicAssert.AreEqual(script, newScriptBytes);
//        }
    }
}
