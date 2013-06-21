using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HathLibraries.DataTypes
{
    public class EHFile
    {
        public static string BuildExpectedLocation(string file)
        {
            return string.Format("{0}{1}\\{2}\\{3}", Configuration.Locations.Cache, file.Substring(0, 1), file.Substring(1, 1), file);
        }

        public override string ToString()
        {
            return this.filename;
        }

        private string location;
        private string filename;

        private string _hash;
        private int _size;
        private int _width;
        private int _height;
        private string _format;

        private string _ahash = null;
        private int _asize = -1;
        private int _awidth = -1;
        private int _aheight = -1;
        private string _aformat = null;

        private bool prefail = false;
        private bool verified = false;

        public string Hash { get { return this._hash; } }
        public int Size { get { return this._size; } }
        public int Width { get { return this._width; } }
        public int Height { get { return this._height; } }
        public string Format { get { return this._format; } }

        public string Name { get { return this.filename; } }
        public string Location { get { return this.location; } }
        public bool PreFail { get { return this.prefail; } }
        public string ExpectedLocation
        {
            get
            {
                return string.Format("{0}{1}\\{2}\\{3}", Configuration.Locations.Cache, this.filename.Substring(0, 1), this.filename.Substring(1, 1), this.filename);
            }
        }

        public string ActualHash
        {
            get
            {
                if (!this.verified)
                    throw new Exception("File was not verified yet.");

                return this._ahash;
            }
        }
        public int ActualSize
        {
            get
            {
                if (!this.verified)
                    throw new Exception("File was not verified yet.");

                return this._asize;
            }
        }
        public int ActualWidth { get { return this._awidth; } }         // to implement
        public int ActualHeight { get { return this._aheight; } }       // to implement
        public string ActualFormat { get { return this._aformat; } }    // to implement

        public EHFile(string Location)
        {
            this.location = Location;
            this.GatherData();
        }

        private void GatherData()
        {
            try
            {
                if (!File.Exists(this.location))
                {
                    this.prefail = true;
                    return;
                }

                this.filename = Path.GetFileName(this.location);
                Match match = Regex.Match(filename, "^([a-z0-9]{40})-([0-9]+)-([0-9]+)-([0-9]+)-(jpg|png|jpeg|gif)$", RegexOptions.Singleline);

                this._hash = match.Groups[1].Value;
                this._size = int.Parse(match.Groups[2].Value);
                this._width = int.Parse(match.Groups[3].Value);
                this._height = int.Parse(match.Groups[4].Value);
                this._format = match.Groups[5].Value;
            }
            catch (Exception Ex)
            {
                Ex.Print(LogType.Debug, true, false);
                this.prefail = true;
            }
        }

        public bool Verify()
        {
            if (this.prefail || this.verified)
                return false;

            try
            {
                byte[] image = File.ReadAllBytes(this.location);
                string filehash = image.HashSHA1();
                int filesize = image.Length;

                this._ahash = filehash;
                this._asize = filesize;

                this.verified = true;

                return
                    filehash == this._hash &&
                    filesize == this._size;
            }
            catch (Exception Ex)
            {
                Ex.Print(LogType.Debug, true, false);
                return false;
            }
        }

        public bool Move(string location = null)
        {
            try
            {
                if (location == null)
                    location = this.ExpectedLocation;

                if (!Directory.Exists(Path.GetDirectoryName(location)))
                    Directory.CreateDirectory(Path.GetDirectoryName(location));

                if (File.Exists(location))
                    File.Delete(this.location);
                else
                    File.Move(this.location, location);

                this.location = location;

                return true;
            }
            catch (Exception Ex)
            {
                Ex.Print(LogType.Error, true, false);
                return false;
            }
        }
    }
}
