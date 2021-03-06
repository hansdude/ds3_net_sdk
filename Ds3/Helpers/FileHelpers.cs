﻿/*
 * ******************************************************************************
 *   Copyright 2014 Spectra Logic Corporation. All Rights Reserved.
 *   Licensed under the Apache License, Version 2.0 (the "License"). You may not use
 *   this file except in compliance with the License. A copy of the License is located at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 *   or in the "license" file accompanying this file.
 *   This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
 *   CONDITIONS OF ANY KIND, either express or implied. See the License for the
 *   specific language governing permissions and limitations under the License.
 * ****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Ds3.Models;

namespace Ds3.Helpers
{
    public class FileHelpers
    {
        /// <summary>
        /// Creates a function that can open a file stream for writing.
        /// 
        /// For use with IJob.Transfer(getter).
        /// 
        /// Creates all directories needed to save the file.
        /// </summary>
        /// <param name="root">The root directory within which to save objects.</param>
        /// <returns></returns>
        public static Func<string, Stream> BuildFileGetter(string root)
        {
            return key =>
            {
                var filePath = Path.Combine(root, ConvertKeyToPath(key));
                EnsureDirectoryForFile(filePath);
                return File.OpenWrite(filePath);
            };
        }

        private static void EnsureDirectoryForFile(string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Creates a function that can open a file stream for reading.
        /// 
        /// For use with IJob.Transfer(putter).
        /// </summary>
        /// <param name="root">The root directory within which to read objects.</param>
        /// <returns></returns>
        public static Func<string, Stream> BuildFilePutter(string root)
        {
            return key => File.OpenRead(Path.Combine(root, ConvertKeyToPath(key)));
        }

        /// <summary>
        /// Returns a list of object key, size pairs for a directory root (recursive).
        ///
        /// For use with IDs3ClientHelpers.StartWriteJob(bucket, objectsToWrite)
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<Ds3Object> ListObjectsForDirectory(string root)
        {
            var rootDirectory = new DirectoryInfo(root);
            var rootSize = rootDirectory.FullName.Length + 1;
            return rootDirectory
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(file => new Ds3Object(
                    ConvertPathToKey(file.FullName.Substring(rootSize)),
                    file.Length
                ));
        }

        private static string ConvertKeyToPath(string key)
        {
            return key.Replace('/', '\\');
        }

        private static string ConvertPathToKey(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}
