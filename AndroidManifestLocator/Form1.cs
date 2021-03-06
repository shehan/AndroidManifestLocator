﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace AndroidManifestLocator
{
    public partial class Form1 : Form
    {
        String rootDirectory;
        StreamWriter outpuFile;

        public Form1()
        {
            InitializeComponent();
        }

        private void DoWork()
        {
            UpdateWorkStatusLog($"Started!");
            outpuFile.WriteLine($"FilePath,PackageName,MaxSdkVersion,MinSdkVersion,TargetSdkVersion,PermissionCount,Permissions");
            outpuFile.Flush();

            var directories = Directory.GetDirectories(rootDirectory);
            using (outpuFile)
            {                
                foreach (var dir in directories)
                {
                    var files = Directory.EnumerateFiles(dir, "AndroidManifest.xml", SearchOption.AllDirectories);
                    // .Where(s => s.ToLower().Equals("androidmanifest.xml"));

                    foreach (var file in files)
                    {
                        using (var fileStream = new StreamReader(File.OpenRead(file), Encoding.UTF8))
                        {
                            var content = fileStream.ReadToEnd();
                            int intMax, intMin, intTarget;

                            var permissions = XMLExtract(content, "uses-permission", "android:name").ToList();
                            var permissionString = String.Join(";", permissions);
                            var permissionCount = permissions.Count;

                            var package = XMLExtract(content, "manifest", "package").FirstOrDefault();
                            var maxSdkVersion = int.TryParse((XMLExtract(content, "uses-sdk", "android:maxSdkVersion").FirstOrDefault()), out intMax) ? intMax : 0;
                            var minSdkVersion = int.TryParse((XMLExtract(content, "uses-sdk", "android:minSdkVersion").FirstOrDefault()), out intMin) ? intMin : 0;
                            var targetSdkVersion = int.TryParse((XMLExtract(content, "uses-sdk", "android:targetSdkVersion").FirstOrDefault()), out intTarget) ? intTarget : 0;

                            UpdateWorkStatusLog($"Processing: {file}");
                            outpuFile.WriteLine($"\"{file}\",\"{package}\",{maxSdkVersion},{minSdkVersion},{targetSdkVersion},{permissionCount},\"{permissionString}\"");
                            outpuFile.Flush();
                        }
                    }
                }
            }

            outpuFile.Close();
            UpdateWorkStatusLog($"Completed!");
            MessageBox.Show("Done!");
        }

        private List<string> XMLExtract(string xml, string node, string attribute)
        {
            List<string> extract = new List<string>();

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                XmlNodeList elemList = doc.GetElementsByTagName(node);
                for (int i = 0; i < elemList.Count; i++)
                {
                    if (elemList[i].Attributes[attribute] != null)
                    {
                        var attrVal = elemList[i].Attributes[attribute].Value;
                        if (!string.IsNullOrEmpty(attrVal))
                            extract.Add(attrVal);
                    }
                }
            }
            catch (Exception e)
            {
                UpdateWorkStatusLog("!!!!!Error.....!!!" + e.ToString());
                return extract;
            }

            return extract;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!Directory.Exists(textBoxPath.Text))
            {
                MessageBox.Show("Direcorty does not exist!");
                return;
            }
            rootDirectory = textBoxPath.Text;
            var outpuFileName = $"Output_{DateTime.Now.Ticks}.csv";
            outpuFile = File.CreateText(outpuFileName);
            UpdateWorkStatusLog($"Output File: {new FileInfo(outpuFileName).FullName}");

            textBoxPath.Enabled = false;
            buttonStart.Enabled = false;
            Thread t = new Thread(DoWork);
            t.Start();
        }

        private void UpdateWorkStatusLog(string text)
        {
            if (this.textBoxLog.InvokeRequired)
            {
                UpdateLogCallback callback = new UpdateLogCallback(UpdateWorkStatusLog);
                this.Invoke(callback, new object[] { text });
            }
            else
            {
                textBoxLog.Focus();
                textBoxLog.AppendText($"{Environment.NewLine}{DateTime.Now.ToString()} - {text}");
            }
        }

        delegate void UpdateLogCallback(string text);
    }
}
