using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;
using Google.Apis.Auth.OAuth2; // Google.Apis.Auth
using Google.Apis.Services;
using Google.Apis.Drive.v3; // Google.Apis.Drive.v3
using System.IO;
using System.IO.Compression;
using System.Net;
using Newtonsoft.Json.Linq;

public class JenkinsBuild
{
    private const string PathToServiceAccountKeyFile = @"D:\Unity_Project\logical-bolt-297107-de7852364e06.json";
    private const string DirectoryId = "NOT PUBLIC";
    static BuildSummary summary;
    
    [MenuItem("Build/Standalone Windows")]
    public static void PerformBuild()
    {
        var args = FindArgs();

        BuildPlayerOptions options = new BuildPlayerOptions();
        // 씬 추가
        List<string> scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;

            scenes.Add(scene.path);
        }
        options.scenes = scenes.ToArray();
        
        // 타겟 경로(빌드 결과물이 여기 생성됨)
        options.locationPathName = $"{args.targetDir}/Project_UB_{args.appName}_{PlayerSettings.bundleVersion}.exe";
        // 빌드 타겟
        options.target = BuildTarget.StandaloneWindows;

        // 빌드
        BuildReport report = BuildPipeline.BuildPlayer(options);
        summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes" + " " + summary.totalTime + " Times");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }
        
        string fileID = DriveCreateFolder($"{args.appName}");
        string PrefilePath = $"{args.targetDir}";
        string filePath = PrefilePath.TrimEnd('\\');
        int lastSeparatorIndex = filePath.LastIndexOf(Path.DirectorySeparatorChar);
        string fileName = filePath.Substring(lastSeparatorIndex + 1);
        string FolderId = DriveCreateFolder(fileName, fileID);
        UploadGoogleDrive(filePath, $"Project_UB_{args.appName}_{PlayerSettings.bundleVersion}", FolderId, fileName);
    }
    
    private static void UploadGoogleDrive(string filePath, string name, string forderID, string fileName)
    {
        var credential = GoogleCredential.FromFile(PathToServiceAccountKeyFile).CreateScoped(DriveService.Scope.Drive);
        // Create Drive API service.
        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
            //ApplicationName = "Drive API Snippets"
        });
    
        int lastSeparatorIndex = filePath.LastIndexOf(Path.DirectorySeparatorChar);
        string parentDirectory = filePath.Substring(0, lastSeparatorIndex);
        string buildNum = filePath.Substring(lastSeparatorIndex + 1);
        string zipPath = parentDirectory + Path.DirectorySeparatorChar + name + $"_{fileName}.zip";
        ZipFile.CreateFromDirectory(filePath, zipPath);
        Directory.Delete(filePath, true);
        // Upload file photo.jpg on drive.
        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = name + $"_{fileName}.zip", //업로드 파일명(드라이브 상에서 보여지는 이름)
            Parents = new List<string>() { forderID }
        };
        FilesResource.CreateMediaUpload request;
        // Create a new file on drive.
        using (var stream = new FileStream(zipPath, FileMode.Open))
        {
            // Create a new file, with metadata and stream.
            request = service.Files.Create(
                fileMetadata, stream, "text/plain");
            request.Fields = "id";
            request.Upload();
        }
        var file = request.ResponseBody;
        // 다운로드 링크 생성
        string downloadLink = $"https://drive.google.com/file/d/{file.Id}/view?usp=drive_link";
        SendDisMessage($"[새로운 빌드] {name}_{fileName}.zip", $"Name : {name}\nBUILD_NUMBER : {buildNum}\nGOOGLE DRIVE FILE NUM : {file.Id}\nTOTAL SIZE : {summary.totalSize}\nTOTAL TIME : {summary.totalTime}", $"{downloadLink}");
    }

    public static string DriveCreateFolder(string FolderName, string fileID = DirectoryId)
    {
        string existingFolderId = GetFolderId(FolderName, fileID);
        if (existingFolderId != null)
        {
            // Folder already exists, return its ID.
            return existingFolderId;
        }
        var credential = GoogleCredential.FromFile(PathToServiceAccountKeyFile)
                        .CreateScoped(DriveService.Scope.Drive);

        // Create Drive API service.
        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            //ApplicationName = "Drive API Snippets"
        });

        // File metadata
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = FolderName, //만들 폴더명
            Parents = new List<string>() { fileID },
            MimeType = "application/vnd.google-apps.folder"
        };

        // Create a new folder on drive.
        var request = service.Files.Create(fileMetadata);
        request.Fields = "id";
        var file = request.Execute();
        // Prints the created folder id.
        //MessageBox.Show("folder id" + file.Id);

        return file.Id;
    }

    public static string GetFolderId(string folderName, string parentId = DirectoryId)
    {
        var credential = GoogleCredential.FromFile(PathToServiceAccountKeyFile)
                            .CreateScoped(DriveService.Scope.Drive);

        // Create Drive API service.
        var service = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
        });

        // Define parameters for the search.
        string query = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and '{parentId}' in parents and trashed=false";
        var request = service.Files.List();
        request.Q = query;
        var result = request.Execute();
        
        // Check if the folder exists.
        if (result.Files != null && result.Files.Count > 0)
        {
            // Return the ID of the first matching folder.
            return result.Files[0].Id;
        }
        else
        {
            // Folder does not exist.
            return null;
        }
    }

    private static Args FindArgs()
    {
        var returnValue = new Args();
 
        // find: -executeMethod
        //   +1: JenkinsBuild.BuildMacOS
        //   +2: FindTheGnome
        //   +3: D:\Jenkins\Builds\Find the Gnome\47\output
        string[] args = System.Environment.GetCommandLineArgs();
        var execMethodArgPos = -1;
        bool allArgsFound = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-executeMethod")
            {
                execMethodArgPos = i;
            }
            var realPos = execMethodArgPos == -1 ? -1 : i - execMethodArgPos - 2;
            if (realPos < 0)
                continue;
 
            if (realPos == 0)
                returnValue.appName = args[i];
            if (realPos == 1)
            {
                returnValue.targetDir = args[i];
                if (!returnValue.targetDir.EndsWith(Path.DirectorySeparatorChar + ""))
                    returnValue.targetDir += Path.DirectorySeparatorChar;
 
                allArgsFound = true;
            }
        }
        
        if (!allArgsFound)
            System.Console.WriteLine("[JenkinsBuild] Incorrect Parameters for -executeMethod Format: -executeMethod JenkinsBuild.BuildWindows64 <app name> <output dir>");
 
        return returnValue;
    }

    private class Args
    {
        public string appName = "UB";
        public string targetDir = @"D:\JenkinsBuild\UB\test\";
    }

    private static void sendDisWebhook(string URL, string json)
    {
        var wr = WebRequest.Create(URL);
        wr.ContentType = "application/json";
        wr.Method = "POST";
        using (var sw = new StreamWriter(wr.GetRequestStream()))
            sw.Write(json);
        wr.GetResponse();
    }

    [MenuItem("Build/SendDiscordWebHook")]
    public static void TestSend()
    {
        SendDisMessage("ASDFASDAS.zip", "웹훅 테스트 입니다.", "https://www.youtube.com/watch?v=NyCtO1bTWkY");
    }

    private static void SendDisMessage(string fileName, string description, string DownloadUrl)
    {
        JObject embedValue = new JObject(
            new JProperty("title", $"{fileName}"),
            new JProperty("description", $"{description}"),
            new JProperty("url", $"{DownloadUrl}"),
            new JProperty("color", 16777215)
        );
        JArray embedArray = new JArray(embedValue);
        JObject json = new JObject {
            new JProperty("content", null),
            new JProperty("embeds", embedArray)
        };
        sendDisWebhook("NOT PUBLIC", json.ToString());
    }
}
