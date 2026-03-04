using System;
using System.Collections.Generic;
using System.Text;

namespace Exporter
{
    public class FtpSettings
    {
        public required string Host { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? RemoteDirectory { get; set; } 
        public string? LocalTempPath { get; set; }
    }
}
