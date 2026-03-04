using System;
using System.Collections.Generic;
using System.Text;

namespace Importer
{
    public class FtpSettings
    {
        public string? Host { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? RemoteDirectory { get; set; }
        public string? LocalTempPath { get; set; }
    }
}
