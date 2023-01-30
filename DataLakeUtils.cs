using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ElasticSearchDataFactory
{

    /// <summary>
    /// Basic data lake interaction processes 
    /// 
    /// Most of this is directly lifted from documentation but a few items are expands specifically to 
    /// get things like metadata properly uploaded.
    /// 
    /// </summary>
    public class DataLakeUtils
    {
        //Mostly taken from https://docs.microsoft.com/en-us/azure/storage/blobs/data-lake-storage-directory-file-acl-dotnet

        // Creation of the FileSystemClient (different than the service client)
        public static DataLakeFileSystemClient GetDataLakeFileSystemClient(Uri FileSystemUrl, Azure.Storage.StorageSharedKeyCredential SharedKey)
        {
            return new DataLakeFileSystemClient(FileSystemUrl, SharedKey);
        }

        // Enumerator to get full list of files in the directory, you do have to manually ignore directories.
        // This will not request the full file list, it is a pointer than makes the call as needed
        public static IAsyncEnumerator<PathItem> ListFilesInDirectory(DataLakeFileSystemClient fileSystemClient, string directoryName)
        {
            return fileSystemClient.GetPathsAsync(directoryName).GetAsyncEnumerator();
        }

        // Creates the directory if it doesn't already exist
        // TODO: Checks to verify it's actually a directory format
        public static async Task CreateDirectory(DataLakeFileSystemClient fileSystemClient, string path)
        {
            DataLakeDirectoryClient directory = fileSystemClient.GetDirectoryClient(path);
            await directory.CreateIfNotExistsAsync();
        }

        // Create the file, upload the data, and add the metadata.
        // TODO: Make the upload and metadata add non-sequential
        public static async Task UploadFileWithMetadata(DataLakeFileSystemClient fileSystemClient, string directory, string fileName, IDictionary<String, String> metadata, Stream fileContents)
        {
            try
            {
                DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(directory);
                DataLakeFileClient fileClient = directoryClient.GetFileClient(fileName);

                // I
                if (!(await fileClient.ExistsAsync()))
                {
                    await fileClient.UploadAsync(fileContents);
                    await fileClient.SetMetadataAsync(metadata);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Create the file client to start working on the new file
        public static DataLakeFileClient CreateFileClient(DataLakeFileSystemClient fileSystemClient, string filePath)
        {
            return fileSystemClient.GetFileClient(filePath);
        }

    }
}
