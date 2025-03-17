using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UB
{
    public class EnumGenerator : Editor
    {
        public static EffectData effectData;

        [InitializeOnLoadMethod]
        [MenuItem("Tools/ProjectUB/EnumGenerate")]
        public static void GenerateEnumFile()
        {
            EffectDataEnum();
            LayersEnum();
            AssetDatabase.Refresh();
        }

        public static void EffectDataEnum()
        {
            effectData = AssetDatabase.LoadAssetAtPath<EffectData>("Assets/Settings/Scriptable Objects/ObjectPool/data-EffectData.asset");

            if (ReferenceEquals(effectData, null))
            {
                Debug.LogAssertion("EffectData is null");
                return;
            }

            string enumName = "EffectEnum";
            string enumFile = "Assets/ProjectUB/Scripts/Enums/" + enumName + ".cs";
            string enumContent = "namespace UB\n{\n\tpublic enum " + enumName + "\n\t{\n";

            List<string> enumValues = new List<string>();

            foreach (var effectData in effectData.playerEffects)
            {
                enumValues.Add(effectData.name);
            }

            foreach (var effectData in effectData.enemyEffects)
            {
                enumValues.Add(effectData.name);
            }

            foreach (var effectData in effectData.environmentEffects)
            {
                enumValues.Add(effectData.name);
            }

            foreach (var effectData in effectData.otherEffects)
            {
                enumValues.Add(effectData.name);
            }

            //enumValues.Sort();

            foreach (string enumValue in enumValues)
            {
                enumContent += "\t\t" + enumValue + ",\n";
            }

            enumContent += "\t}\n}";

            Directory.CreateDirectory(Path.GetDirectoryName(enumFile));

            #region Legacy
            // // 임시 경로 사용
            // string tempFilePath = Path.GetTempPath() + enumName + ".cs";
            // File.WriteAllText(tempFilePath, enumContent);
            //
            // // 원하는 경로로 파일 복사
            // File.Copy(tempFilePath, enumFile, true);
            // File.Delete(tempFilePath); // 임시 파일 삭제
            #endregion
            
            using (StreamWriter streamWriter = new StreamWriter(enumFile))
            {
                streamWriter.Write(enumContent);
            }
        }
        
        public static void LayersEnum()
        {
            string enumName = "LayerEnum";
            string filePath = "Assets/ProjectUB/Scripts/Enums/" + enumName + ".cs";

            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.WriteLine("namespace UB\n{\n\tpublic enum " + enumName);
                streamWriter.WriteLine("\t{");

                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);

                    if (!string.IsNullOrEmpty(layerName))
                    {
                        streamWriter.WriteLine("\t\t" + layerName.Replace(" ", "_") + ",");
                    }
                }
                streamWriter.WriteLine("\t}\n}");
            }
        }
    }
}
