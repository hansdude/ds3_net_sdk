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
using Ds3.Models;
using Ds3.Runtime;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Ds3.ResponseParsers
{
    internal class GetPhysicalPlacementResponseParser : IResponseParser<GetPhysicalPlacementRequest, GetPhysicalPlacementResponse>
    {
        public GetPhysicalPlacementResponse Parse(GetPhysicalPlacementRequest request, IWebResponse response)
        {
            using (response)
            {
                ResponseParseUtilities.HandleStatusCode(response, HttpStatusCode.OK);
                using (var stream = response.GetResponseStream())
                {
                    var dataNode = XmlExtensions.ReadDocument(stream).ElementOrThrow("Data");
                    if (request.FullDetails)
                    {
                        return new GetPhysicalPlacementResponse(dataNode.Elements("Object").Select(ParseObjectPlacement).ToList());
                    }
                    else
                    {
                        return new GetPhysicalPlacementResponse(dataNode.ElementOrThrow("Tapes").Elements("Tape").Select(ParseTape).ToList());
                    }
                }
            }
        }

        private static Ds3ObjectPlacement ParseObjectPlacement(XElement objectEl)
        {
            return new Ds3ObjectPlacement
            {
                 Name = objectEl.AttributeText("Name"),
                 Offset = long.Parse(objectEl.AttributeText("Offset")),
                 Length = long.Parse(objectEl.AttributeText("Length")),
                 Tapes = objectEl
                     .ElementOrThrow("PhysicalPlacement")
                     .ElementOrThrow("Tapes")
                     .Elements("Tape")
                     .Select(ParseTape)
            };
        }

        private static Tape ParseTape(XElement tapeEl)
        {
            return new Tape
            {
                AssignedToBucket = bool.Parse(tapeEl.TextOf("AssignedToBucket")),
                AvailableRawCapacity = ParseNullableLong(tapeEl.TextOfOrNull("AvailableRawCapacity")),
                BarCode = tapeEl.TextOf("BarCode"),
                BucketId = tapeEl.TextOfOrNull("BucketId"),
                DescriptionForIdentification = tapeEl.TextOfOrNull("DescriptionForIdentification"),
                EjectDate = ParseNullableDateTime(tapeEl.TextOfOrNull("EjectDate")),
                EjectLabel = tapeEl.TextOfOrNull("EjectLabel"),
                EjectLocation = tapeEl.TextOfOrNull("EjectLocation"),
                EjectPending = ParseNullableDateTime(tapeEl.TextOfOrNull("EjectPending")),
                FullOfData = bool.Parse(tapeEl.TextOf("FullOfData")),
                Id = tapeEl.TextOf("Id"),
                LastAccessed = ParseNullableDateTime(tapeEl.TextOfOrNull("LastAccessed")),
                LastCheckpoint = tapeEl.TextOfOrNull("LastCheckpoint"),
                LastModified = ParseNullableDateTime(tapeEl.TextOfOrNull("LastModified")),
                LastVerified = ParseNullableDateTime(tapeEl.TextOfOrNull("LastVerified")),
                PartitionId = tapeEl.TextOfOrNull("PartitionId"),
                PreviousState = ParseNullableTapeState(tapeEl.TextOfOrNull("PreviousState")),
                SerialNumber = tapeEl.TextOfOrNull("SerialNumber"),
                State = ParseTapeState(tapeEl.TextOf("State")),
                TotalRawCapacity = ParseNullableLong(tapeEl.TextOfOrNull("TotalRawCapacity")),
                Type = ParseTapeType(tapeEl.TextOf("Type")),
                WriteProtected = bool.Parse(tapeEl.TextOf("WriteProtected")),
            };
        }

        private static DateTime? ParseNullableDateTime(string dateTimeStringOrNull)
        {
            return string.IsNullOrWhiteSpace(dateTimeStringOrNull) ? (DateTime?)null : DateTime.Parse(dateTimeStringOrNull);
        }

        private static long? ParseNullableLong(string longOrNull)
        {
            return string.IsNullOrWhiteSpace(longOrNull) ? (long?)null : long.Parse(longOrNull);
        }

        private static TapeState? ParseNullableTapeState(string tapeStateOrNull)
        {
            return string.IsNullOrWhiteSpace(tapeStateOrNull) ? (TapeState?)null : ParseTapeState(tapeStateOrNull);
        }

        private static TapeState ParseTapeState(string tapeState)
        {
            return ParseEnumType<TapeState>(tapeState);
        }

        private static TapeType ParseTapeType(string tapeType)
        {
            return ParseEnumType<TapeType>(tapeType);
        }

        private static T ParseEnumType<T>(string enumString)
            where T : struct
        {
            T result;
            if (!Enum.TryParse(ConvertToPascalCase(enumString), out result))
            {
                throw new ArgumentException("Invalid value for type: " + typeof(T).Name);//TODO:i8l
            }
            return result;
        }

        private static string ConvertToPascalCase(string uppercaseUnderscore)
        {
            var sb = new StringBuilder();
            foreach (var word in uppercaseUnderscore.Split('_'))
            {
                sb.Append(word[0]);
                sb.Append(word.Substring(1).ToLowerInvariant());
            }
            return sb.ToString();
        }
    }
}
