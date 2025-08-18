#if UNITY_EDITOR && UNITY_ANDROID
using UnityEngine;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;

namespace UMI {
    public class AndroidManifestPatcher : IPostGenerateGradleAndroidProject {
        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path) {
            XNamespace android = "http://schemas.android.com/apk/res/android";
            var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");

            if (!File.Exists(manifestPath)) {
                WarnAboutFailure("Unable to locate and patch your Android Manifest");
                return;
            }

            var doc = XDocument.Load(manifestPath);

            // Find the first activity that has an intent-filter with MAIN + LAUNCHER
            // This works for both classic Activity and GameActivity (2023.1+) application entry
            var activity = doc
                .Descendants("activity")
                .FirstOrDefault(a =>
                    a.Elements("intent-filter").Any(f =>
                        f.Elements("action")
                        .Any(x => (string)x.Attribute(android + "name") == "android.intent.action.MAIN") &&
                        f.Elements("category")
                        .Any(x => (string)x.Attribute(android + "name") == "android.intent.category.LAUNCHER")
                    )
                );

            if (activity == null) {
                WarnAboutFailure("Unable to locate the main launcher activity in your AndroidManifest.xml");
                return;
            }
            activity.SetAttributeValue(android + "windowSoftInputMode", "adjustNothing");
            doc.Save(manifestPath);
#if UMI_DEBUG
            Debug.Log($"[UMI] Patched AndroidManifest.xml at {manifestPath} with 'adjustNothing' for windowSoftInputMode.");
#endif

            void WarnAboutFailure(string reason) {
                Debug.LogWarning($"[UMI] {reason}. Please refer to the documentation and ensure you've updated your AndroidManifest.xml manually with the required changes.");
            }
        }
    }
}
#endif
