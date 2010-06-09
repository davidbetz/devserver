#region Copyright
//+ Copyright © Jampad Technology, Inc. 2007-2008
//++ Lead Architect: David Betz [MVP] <dfb/davidbetz/net>
#endregion
using System;
using System.IO;
//+
namespace Themelia.IO
{
    public static class StreamConverter
    {
        //- @CreateStream -//
        /// <summary>
        /// Creates the stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static T CreateStream<T>(String text) where T : Stream, new()
        {
            T stream = new T();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(text);
            //+
            return stream;
        }
        /// <summary>
        /// Creates the stream.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static T CreateStream<T>(Byte[] data) where T : Stream, new()
        {
            T stream = new T();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(data);
            //+
            return stream;
        }

        //- @GetStreamByteArray -//
        /// <summary>
        /// Gets the stream byte array.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static Byte[] GetStreamByteArray(Stream stream)
        {
            if (stream != null)
            {
                BinaryReader r = new BinaryReader(stream);
                //+
                return r.ReadBytes((Int32)stream.Length);
            }
            //+
            return null;
        }

        //- @GetStreamText -//
        /// <summary>
        /// Gets the stream text.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns></returns>
        public static String GetStreamText(Stream stream)
        {
            if (stream != null)
            {
                StreamReader reader = new StreamReader(stream);
                //+
                return reader.ReadToEnd();
            }
            //+
            return String.Empty;
        }
    }
}