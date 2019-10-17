// This file is part of PropertyChanged.Fody.Analyzer.
//
// PropertyChanged.Fody.Analyzer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PropertyChanged.Fody.Analyzer is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PropertyChanged.Fody.Analyzer.  If not, see <https://www.gnu.org/licenses/>.

namespace PropertyChanged.Fody.Analyzer.Test.Helpers
{
    using System;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Location where the diagnostic appears, as determined by path, line number, and column number.
    /// </summary>
    public struct DiagnosticResultLocation
    {
        public DiagnosticResultLocation(string path, int line, int column)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "column must be >= -1");
            }

            Path = path;
            Line = line;
            Column = column;
        }

        public string Path { get; }

        public int Line { get; }

        public int Column { get; }
    }

    /// <summary>
    /// Struct that stores information about a Diagnostic appearing in a source
    /// </summary>
    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] locations;

        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (locations == null)
                {
                    locations = new DiagnosticResultLocation[] { };
                }

                return locations;
            }

            set { locations = value; }
        }

        public DiagnosticSeverity Severity { get; set; }

        public string Id { get; set; }

        public string Message { get; set; }

        public string Path
        {
            get { return Locations.Length > 0 ? Locations[0].Path : ""; }
        }

        public int Line
        {
            get { return Locations.Length > 0 ? Locations[0].Line : -1; }
        }

        public int Column
        {
            get { return Locations.Length > 0 ? Locations[0].Column : -1; }
        }
    }
}