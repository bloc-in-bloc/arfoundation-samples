using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Possible.Vision.Editor {
    public static class VisionPostProcess {
        [PostProcessBuild]
        public static void OnPostProcessBuild (BuildTarget buildTarget, string buildPath) {
            if (buildTarget == BuildTarget.iOS) {
                PBXProject pbxProject = new PBXProject ();
                string projectPath = PBXProject.GetPBXProjectPath (buildPath);
                pbxProject.ReadFromFile (projectPath);
                
                string unityFrameworkGuid = pbxProject.TargetGuidByName ("UnityFramework");

                pbxProject.SetBuildProperty(unityFrameworkGuid, "ENABLE_BITCODE", "NO");
                // string headerPath = pbxProject.FindFileGuidByProjectPath ("Libraries/unity-vision/Scripts/Possible/Vision/Plugins/iOS/Vision-Bridging-Header.h");
                // if (headerPath == null) {
                //     Debug.LogError ("Can't find Vision-Bridging-Header.h");
                // }
                // pbxProject.AddPublicHeaderToBuild (unityFrameworkGuid, "Libraries/unity-vision/Scripts/Possible/Vision/Plugins/iOS/Vision-Bridging-Header.h");
                pbxProject.SetBuildProperty(unityFrameworkGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "Vision-Swift.h");
                pbxProject.AddBuildProperty(unityFrameworkGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks $(PROJECT_DIR)/lib/$(CONFIGURATION) $(inherited)");
                pbxProject.AddBuildProperty(unityFrameworkGuid, "FRAMEWORK_SEARCH_PATHS", "$(inherited) $(PROJECT_DIR) $(PROJECT_DIR)/Frameworks");
                pbxProject.AddBuildProperty(unityFrameworkGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
                
                string stringPbxProject = pbxProject.WriteToString ();
                File.WriteAllText (projectPath, stringPbxProject);
                // pbxProject.WriteToFile(buildPath);
                
                // proj.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");
                // proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_BRIDGING_HEADER", "Libraries/Scripts/Possible/Vision/Plugins/iOS/Vision-Bridging-Header.h");
                // proj.SetBuildProperty(targetGuid, "SWIFT_OBJC_INTERFACE_HEADER_NAME", "Vision-Swift.h");
                // proj.AddBuildProperty(targetGuid, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks $(PROJECT_DIR)/lib/$(CONFIGURATION) $(inherited)");
                // proj.AddBuildProperty(targetGuid, "FRAMEWORK_SEARCH_PATHS", "$(inherited) $(PROJECT_DIR) $(PROJECT_DIR)/Frameworks");
                // proj.AddBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
                // proj.AddBuildProperty(targetGuid, "DEFINES_MODULE", "YES");
                // // proj.AddBuildProperty(targetGuid, "SWIFT_VERSION", "4.0");
                // proj.WriteToFile(projPath);
            }
        }
    }
}