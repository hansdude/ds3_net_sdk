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

using Ds3.Calls;
using Ds3.Helpers.RangeTranslators;
using Ds3.Helpers.TransferItemSources;
using Ds3.Helpers.Transferrers;
using Ds3.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ds3.Helpers.Jobs
{
    internal class FullObjectJob : Job<IJob, string>, IJob
    {
        public static IJob Create(
            JobResponse jobResponse,
            ITransferItemSource transferItemSource,
            ITransferrer transferrer)
        {
            var blobs = Blob.Convert(jobResponse);
            var rangesForRequests = blobs.ToLookup(b => b, b => b.Range);
            return new FullObjectJob(
                jobResponse.BucketName,
                jobResponse.JobId,
                transferItemSource,
                transferrer,
                rangesForRequests,
                blobs
            );
        }

        public FullObjectJob(
            string bucketName,
            Guid jobId,
            ITransferItemSource transferItemSource,
            ITransferrer transferrer,
            ILookup<Blob, Range> rangesForRequests,
            IEnumerable<ContextRange<string>> itemsToTrack)
                : base(
                    bucketName,
                    jobId,
                    transferItemSource,
                    transferrer,
                    rangesForRequests,
                    new RequestToObjectRangeTranslator(rangesForRequests),
                    itemsToTrack
                )
        {
        }
    }
}
