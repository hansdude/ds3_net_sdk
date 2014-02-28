# DS3 .Net SDK

---

A SDK conforming to the SD3 specification.

## Installing

---

The SDK is distributed as a DLL.  The DLL can be added as a Reference into Visual Studio.

## DSK

---

The SDK provides an interface to communicate with a DS3 compliant appliance.  The primary class that is used to interact with DS3 is the `Ds3Client` class.  The `Ds3Client` class is located in the `Ds3` namespace and is used to communicate with DS3.  Here is an example using the Ds3Client class that lists all the buckets on a remote DS3 appliance.

```csharp

using System;

using Ds3;
using Ds3.Models;

namespace Ds3Example
{

  class ListBuckets
  {
    static void Main(string[] args)
    {
      Ds3Client client = new Ds3Client("http://ds3hostname:8080", new Credentials("accessKey", "secretKey"));

      GetServiceResponse response = client.GetService(new GetServiceRequest());

      foreach(Bucket bucket in response.Buckets)
      {
        Console.WriteLine(bucket.Name);
      }
    }
  }
}

```

The SDK allows you to fully communicate with a DS3 appliance.  Each command has a Synchronous version and an Asynchronous version.  The following is a list of all the commands that can be used to communicate with a DS3 appliance.

* `GetService`
* * Args: `GetServiceRequest` - Takes no arguments.
* * Returns: `GetServiceResponse` - Contains a `Buckets` property which lists out all the buckets the current user has access to.