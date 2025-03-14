﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YAXLib;
using System.IO;

namespace Xv2CoreLib.QML
{
    public class Deserializer
    {
        string saveLocation;
        QML_File qml_File;
        public List<byte> bytes = new List<byte>() { 35, 81, 77, 76, 254, 255, 16, 0, 0, 0, 0, 0 };

        public Deserializer(string location) 
        {
            saveLocation = String.Format("{0}/{1}", Path.GetDirectoryName(location), Path.GetFileNameWithoutExtension(location));
            YAXSerializer serializer = new YAXSerializer(typeof(QML_File), YAXSerializationOptions.DontSerializeNullObjects);
            qml_File = (QML_File)serializer.DeserializeFromFile(location);
            Validation();
            WriteBinaryFile();

            File.WriteAllBytes(saveLocation, bytes.ToArray());
        }

        /// <summary>
        /// Create new QML file from a QML_File object. 
        /// </summary>
        public Deserializer(QML_File _qmlFile, string saveLocation) 
        {
            this.saveLocation = saveLocation;
            qml_File = _qmlFile;
            WriteBinaryFile();

            File.WriteAllBytes(saveLocation, bytes.ToArray());
        }

        public Deserializer(QML_File _qmlFile)
        {
            qml_File = _qmlFile;
            WriteBinaryFile();
        }

        private void Validation()
        {
            int entryCount = qml_File.Entries.Count();

            for (int i = 0; i < entryCount; i++)
            {
                Assertion.AssertArraySize(qml_File.Entries[i].I_48, 5, "QML_Entry", "I_48");
            }

        }

        private void WriteBinaryFile()
        {
            int entryCount = qml_File.Entries.Count();

            bytes.AddRange(BitConverter.GetBytes(bytes.Count() + 4));

            for (int i = 0; i < entryCount; i++) 
            {
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SortID));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_04));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_08));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_12));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_16));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_20));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_24));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_28));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_32));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_36));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_40));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_44));

                for (int a = 0; a < 5; a++) 
                {
                    bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].I_48[a]));
                }

                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_00));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_02));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_04));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_06));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_08));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_10));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_12));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_14));
                bytes.AddRange(BitConverter.GetBytes(qml_File.Entries[i].SkillSet.I_16));

            }

            bytes = Utils.ReplaceRange(bytes, BitConverter.GetBytes(entryCount), 8);

        }

    }
}
