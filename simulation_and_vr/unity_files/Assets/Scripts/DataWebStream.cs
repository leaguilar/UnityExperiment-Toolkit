using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using UnityTools.Components;

namespace Assets.Scripts
{
    public sealed class DataWebStream : IDataWriter, IDisposable
    {
        private const int Kb = 1024;

        private const int Mb = 1024 * 1024;

        [NotNull]
        private readonly string url;

        [NotNull]
        private readonly List<char> queuedData;

        [NotNull]
        private char[] conversionBuffer;

        private int conversionBufferSize;

        private bool isClosed;

        [CanBeNull]
        private UnityWebRequest activeRequest;

        public DataWebStream([NotNull]string url, uint minPackageSize = 16 * Kb)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("The url cannot be empty", nameof(url));
            }

            if (minPackageSize < 32)
            {
                minPackageSize = 32;
            }

            this.url = url;

            queuedData = new List<char>(1 * Mb);
            conversionBuffer = new char[minPackageSize / 2];

            CallProvider.AddUpdateListener(SendAllData);
        }

        public void Write(char character)
        {
           queuedData.Add(character);
        }

        public void Write(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            queuedData.AddRange(text);
        }

        public void Write(object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            queuedData.AddRange(data.ToString());
        }

        public void WriteLine()
        {
            queuedData.AddRange(Environment.NewLine);
        }

        public void WriteLine(char character)
        {
            queuedData.Add(character);
            queuedData.AddRange(Environment.NewLine);
        }

        public void WriteLine(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            queuedData.AddRange(text);
            queuedData.AddRange(Environment.NewLine);
        }

        public void WriteLine(object data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            queuedData.AddRange(data.ToString());
            queuedData.AddRange(Environment.NewLine);
        }

        /// <summary>
        /// Sends the rest of the data and then closes the connection.
        /// </summary>
        public void Close()
        {
            if (isClosed)
            {
                throw new InvalidOperationException("The data web stream is already closed.");
            }

            SendAllData();

            CallProvider.RemoveUpdateListener(SendAllData);
            isClosed = true;
        }

        /// <summary>
        /// Disposes the data web stream without sending queued data first.
        /// </summary>
        public void Dispose()
        {
            CallProvider.RemoveUpdateListener(SendAllData);
            isClosed = true; 
        }

        private void SendData()
        {
            if (isClosed)
            {
                throw new InvalidOperationException("The data web stream is already closed.");
            }

            if (activeRequest != null)
            {
                if (activeRequest.isNetworkError)
                {
                    // Error, setup state for retry.
                    Debug.LogError("The web request failed to send data duo to a network error:\n" +
                                   activeRequest.error);

                    // Retry sending data
                    queuedData.InsertRange(0, conversionBuffer);
                    activeRequest = null;
                }
                else if (activeRequest.isHttpError)
                {
                    // Error, setup state for retry.
                    Debug.LogError("The web request failed to send data duo to a http error:\n" +
                                   activeRequest.error);
                 
                    // Retry sending data
                    queuedData.InsertRange(0, conversionBuffer);
                    activeRequest = null;
                }
                else if (activeRequest.isDone)
                {
                    // The web request is finished, prepare data for next one
                    Debug.Log("Data sent successfully:\n" + activeRequest.downloadHandler.text);
                    activeRequest = null;
                }
            }

            if (activeRequest != null || queuedData.Count < conversionBuffer.Length)
            {
                // Not finished sending or not enough data to send yet.
                return;
            }

            queuedData.CopyTo(0, conversionBuffer, 0, conversionBuffer.Length);
            conversionBufferSize = conversionBuffer.Length;
            queuedData.RemoveRange(0, conversionBufferSize);

            var sendBuffer = Encoding.UTF8.GetBytes(conversionBuffer, 0, conversionBufferSize);

            // Prepare a new request with the next data and start it
            activeRequest = new UnityWebRequest(url, "POST");
            activeRequest.uploadHandler = new UploadHandlerRaw(sendBuffer);
            activeRequest.downloadHandler = new DownloadHandlerBuffer();
            activeRequest.certificateHandler = new BypassCertificate();
            activeRequest.SetRequestHeader("Content-Type", "application/json");
            
            // The async operation is handled by unity, we use activeRequest.isDone to check progress.
            activeRequest.SendWebRequest();
            // TODO Deal with errors
            if (activeRequest.isNetworkError || activeRequest.isHttpError)
            {
                Debug.Log(activeRequest.error);
            }
        }

        private void SendAllData()
        {
            var buffer = conversionBuffer;

            if (buffer.Length < queuedData.Count)
            {
                buffer = new char[queuedData.Count];
            }

            queuedData.CopyTo(buffer);
            queuedData.Clear();

            var sendBuffer = Encoding.UTF8.GetBytes(buffer);

            // Send the combined data
            activeRequest = new UnityWebRequest(url, "POST");
            activeRequest.uploadHandler = new UploadHandlerRaw(sendBuffer);
            activeRequest.downloadHandler = new DownloadHandlerBuffer();
            activeRequest.certificateHandler = new BypassCertificate();
            activeRequest.SetRequestHeader("Content-Type", "application/json");
            activeRequest.SendWebRequest();
            // TODO Deal with errors
            if (activeRequest.isNetworkError || activeRequest.isHttpError)
            {
                Debug.Log(activeRequest.error);
            }
        }
        // TODO Move this away, duplicate code
        private class BypassCertificate : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                //Simply return true no matter what
                return true;
            }
        }
        
    }
}
