﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace XnaTools.Spout
{
    public abstract class SpoutThing : GameComponent
    {
        protected const string SenderNamesMMF = "SpoutSenderNames";
        protected const string ActiveSenderMMF = "";
        protected const int SpoutWaitTimeout = 100;
        public const int MaxSenders = 10;
        public const int SenderNameLength = 256;

        protected MemoryMappedFile sharedMemory;
        protected MemoryMappedViewStream sharedMemoryStream;
        protected RenderTarget2D frame;
        protected GraphicsDevice gfxDevice;
        protected TextureDesc textureDesc;
        protected string senderName;

        public RenderTarget2D Frame
        {
            get { return frame; }
        }
        
        public virtual string SenderName
        {
            get { return senderName; }
            set { senderName = value; }
        }

        public SpoutThing(Game game)
            :base(game)
        {

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (frame != null)
                frame.Dispose();
            if (sharedMemoryStream != null)
                sharedMemoryStream.Dispose();
            if (sharedMemory != null)
                sharedMemory.Dispose();
        }

        public List<string> GetSenderNames()
        {
            int len = MaxSenders * SenderNameLength;
            //Read the memory mapped file in to a byte array and close this shit.
            MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen(SenderNamesMMF, len);
            MemoryMappedViewStream stream = mmf.CreateViewStream();
            byte[] b = new byte[len];
            stream.Read(b, 0, len);
            stream.Dispose();
            mmf.Dispose();

            //split into strings searching for the nulls 
            List<string> namesList = new List<string>();
            StringBuilder name = new StringBuilder();
            for (int i=0;i<len;i++)
            {
                if (b[i] == 0)
                {
                    if (name.Length == 0)
                    {
                        i += SenderNameLength - (i % SenderNameLength) -1;
                        continue;
                    }
                    namesList.Add(name.ToString());
                    name.Clear();
                }
                else
                    name.Append((char)b[i]);                    
            }
            return namesList;
        }

        protected byte[] GetNameBytes(string name)
        {
            byte[] b = new byte[SenderNameLength];
            byte[] nameBytes = Encoding.ASCII.GetBytes(name);
            Array.Copy(nameBytes, b, nameBytes.Length);
            return b;
        }
    }
}
