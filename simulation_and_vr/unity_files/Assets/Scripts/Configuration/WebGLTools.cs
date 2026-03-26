using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace Assets.Scripts
{
    [Serializable]
    public class ConfigData
    {
        public string dataAssemblyUrl;
    }

    public static class WebGLTools
    {
        private const string DataAssemblyDebugUrl = "http://www.someserver.com/experiment.html?ExpID=123ABCabc&arg1=bla&arg2=bla&group=2&assignmentId=3QFUFYSY9YFCI8WGZIK4XFTN5K7F4F&hitId=3VLL1PIENQOK4YZ5TCOJQLJUV48ZOH&workerId=A25HNWLO6PL0KH&turkSubmitTo=https%3A%2F%2Fworkersandbox.mturk.com";
        private const string validDeployment = "http://eth-cog.s3.us-east-2.amazonaws.com/experiments/ZollMTURK/index.html";
        public const string configJsonUrl = "/config.json";
        //public const string configJsonUrl = "http://zoll-experiment.s3.amazonaws.com/config.json";
        public static ConfigData myconfig;  
        
        private static List<(string key, string value)> parameters;

        [CanBeNull]
        public static string GetParameter(string key, StringComparison comparison = StringComparison.InvariantCulture)
        {
            if (parameters == null)
            {
                LoadParameters();
            }

            foreach (var parameter in parameters)
            {
                if (string.Equals(parameter.key, key, comparison))
                {
                    return parameter.value;
                }
            }

            return null;
        }

        private static void LoadParameters()
        {
            parameters = new List<(string, string)>();

            var url = Application.isEditor ? DataAssemblyDebugUrl : Application.absoluteURL;

            if (string.IsNullOrWhiteSpace(url))
            {
                // No url available
                return;
            }

            var parameterStart = url.IndexOf('?');

            if (parameterStart < 0)
            {
                // No parameters available
                return;
            }

            var parameterStrings = url.Substring(parameterStart + 1).Split('&');

            foreach (var parameterString in parameterStrings)
            {
                if (string.IsNullOrWhiteSpace(parameterString))
                {
                    // Invalid parameter
                    continue;
                }

                var separator = parameterString.IndexOf('=');
                if (separator <= 0 || separator >= parameterString.Length - 1)
                {
                    // Either key or value are empty, ignore
                    continue;
                }

                var key = parameterString.Substring(0, separator);
                var value = parameterString.Substring(separator + 1, parameterString.Length - separator - 1);

                if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value) || 
                    key.Contains('=') || value.Contains('='))
                {
                    // Key or value are invalid, ignore
                    continue;
                }

                parameters.Add((key, value));
            }
        }

        public static bool ValidateDeployment()
        {
            // TODO put it in the proper place and proper logic
            // Get all information
            // string DeploymentAddress= "any"
            // we can add communication with the server in the future (i.e. am I ready to receive results? I do not recognize you as a hostile user?)
            // Maybe use md5(salt + url)
            bool allowAny=true;
            string DeploymentAddress= allowAny ? "any" : validDeployment; 
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                string runlocation=Application.absoluteURL.Split('?')[0];
                if (DeploymentAddress!="any")
                {
                    if(DeploymentAddress!=runlocation){
                        // TODO
                        // Deal with someone trying to run the experiment somewhere else
                        // Display error code?
                        Debug.Log("Invalid Deployment");
                        return false;
                    }
                }            
            }
            return true;
        }

 
        public static IEnumerator FetchConfigJsonData()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(configJsonUrl))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();
    
                if (webRequest.isNetworkError)
                {
                    Debug.LogError($"Failed to fetch config data due to network fault.\nError: {webRequest.error}");
                }
                else if (webRequest.isHttpError)
                {
                    Debug.LogError($"Failed to fetch config data, response code {webRequest.responseCode}.\nResponse:\n{webRequest.downloadHandler.text}");
                }
                else
                {
                    myconfig = JsonUtility.FromJson<ConfigData>(webRequest.downloadHandler.text);
                }
            }
        }
    }

    

}

 
