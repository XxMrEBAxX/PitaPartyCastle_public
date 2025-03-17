using System.Collections.Generic;
using UB.EVENT;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UB
{
    public class StageObjectGUID : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            OnStageObjectGUIDInitialize();
        }

        [MenuItem("Tools/ProjectUB/StageObjectGUIDInitialize")]
        public static void OnStageObjectGUIDInitialize()
        {
            Dictionary<string, string> stageNames = new Dictionary<string, string>();

            foreach (var scene in EditorBuildSettings.scenes)
            {
                Debug.Log("path : " + scene.path);
                Scene OpenScene = EditorSceneManager.OpenScene(scene.path);
                Debug.Log("open Success! : " + scene);
                foreach (var root in OpenScene.GetRootGameObjects())
                {
                    foreach (var stageObject in root.GetComponentsInChildren<StageInteractive>())
                    {
                        if(stageObject.StageInitializeTypeInstance != StageInitializeType.SAVE)
                        {
                            stageObject.Guid = "";
                            EditorUtility.SetDirty(stageObject);
                            continue;
                        }
                        
                        if(!string.IsNullOrEmpty(stageObject.Guid))
                        {
                            Debug.Log("Exist GUID : " + stageObject.Guid);
                            continue;
                        }

                        stageObject.Guid = GUID.Generate().ToString();
                        Debug.Log("GUID : " + stageObject.Guid);
                        EditorUtility.SetDirty(stageObject);
                    }

                    foreach (var stageObject in root.GetComponentsInChildren<StageSelector>())
                    {
                        if (stageNames.ContainsKey(stageObject.StageName))
                        {
                            string error = stageNames[stageObject.StageName] + "씬의 스테이지 이름과 " + OpenScene.name + "씬의 이름이 중복됩니다!! stageName : " + stageObject.StageName;
                            Debug.LogError(error);
                            //throw new BuildFailedException(error);
                            //continue;
                        }
                        if(!string.IsNullOrEmpty(stageObject.StageName))
                            stageNames.Add(stageObject.StageName, OpenScene.name);
                    }
                }
                EditorSceneManager.SaveScene(OpenScene);
            }
        }
    }
}