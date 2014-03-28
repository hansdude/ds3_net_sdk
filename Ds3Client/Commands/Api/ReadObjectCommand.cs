﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Management.Automation;
using IOFile = System.IO.File;
using Ds3Client.Api;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Ds3Client.Commands.Api
{
    [Cmdlet(VerbsCommunications.Read, DS3Nouns.Object)]
    public class ReadObjectCommand : BaseApiCommand
    {
        private const string ToLocalFileParamSet = "ToLocalFileParamSet";
        private const string ToLocalFolderParamSet = "ToLocalFolderParamSet";

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, ValueFromPipeline = true)]
        public string BucketName { get; set; }

        [Alias(new string[] { "Name" })]
        [Parameter(Position = 1, ParameterSetName = ToLocalFileParamSet, ValueFromPipelineByPropertyName = true, Mandatory = true)]
        public string Key { get; set; }

        [Parameter(Position = 2, ParameterSetName = ToLocalFileParamSet, Mandatory = true)]
        public string File { get; set; }

        [Alias(new string[] { "Prefix" })]
        [Parameter(Position = 1, ParameterSetName = ToLocalFolderParamSet, ValueFromPipelineByPropertyName = true)]
        public string KeyPrefix { get; set; }

        [Alias(new string[] { "Directory" })]
        [Parameter(Position = 2, ParameterSetName = ToLocalFolderParamSet, Mandatory = true)]
        public string Folder { get; set; }

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case ToLocalFileParamSet: WriteToLocalFile(); break;
                case ToLocalFolderParamSet: WriteToLocalFolder(); break;
                default: throw new ApiException(Resources.InvalidParameterSetException);
            }
        }

        private void WriteToLocalFile()
        {
            if (IOFile.Exists(File))
                throw new ApiException(Resources.FileAlreadyExistsException, File);

            WriteObjectToFile(CreateClient(), Key, MakeValidPath(File));
        }

        private void WriteToLocalFolder()
        {
            if (Directory.Exists(Folder))
                throw new ApiException(Resources.DirectoryAlreadyExistsException, Folder);

            if (KeyPrefix != null)
                throw new NotImplementedException(Resources.KeyPrefixNotImplementedException);

            var client = CreateClient();
            using (var response = client.GetBucket(new Ds3.Models.GetBucketRequest(BucketName)))
            using (var bulkGet = client.BulkGet(new Ds3.Models.BulkGetRequest(BucketName, response.Objects)))
            {
                Parallel.ForEach(bulkGet.ObjectLists, ds3ObjectList =>
                {
                    foreach (var key in from ds3Object in ds3ObjectList select MakeValidPath(ds3Object.Name))
                        WriteObjectToFile(client, key, EnsureDirectoryForFileExists(Path.Combine(Folder, key)));
                });
            }
        }

        private static object _ensureDirectoryLock = new object();

        private string EnsureDirectoryForFileExists(string filePath)
        {
            var destinationDirectory = Path.GetDirectoryName(filePath);
            lock(_ensureDirectoryLock)
            {
                if (!Directory.Exists(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);
            }
            return filePath;
        }

        private void WriteObjectToFile(Ds3.Ds3Client client, string key, string file)
        {
            using (var response = client.GetObject(new Ds3.Models.GetObjectRequest(BucketName, key)))
            using (var outputStream = IOFile.OpenWrite(file))
                response.Contents.CopyTo(outputStream);
        }

        private static string MakeValidPath(string path)
        {
            return string.Join("\\", path.Split('/', '\\').Select(MakeValidFileName).ToArray());
        }

        private static string MakeValidFileName(string name)
        {
            return Regex.Replace(name, string.Format(@"([{0}]*\.+$)|([{0}]+)", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), "_");
        }
    }
}