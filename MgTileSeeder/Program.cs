﻿#region Disclaimer / License

// Copyright (C) 2017, Jackie Ng
// https://github.com/jumpinjackie/mapguide-maestro
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

#endregion Disclaimer / License

using CommandLine;
using OSGeo.MapGuide.MaestroAPI;
using OSGeo.MapGuide.MaestroAPI.Commands;
using OSGeo.MapGuide.MaestroAPI.Services;
using OSGeo.MapGuide.MaestroAPI.Tile;
using OSGeo.MapGuide.ObjectModels;
using OSGeo.MapGuide.ObjectModels.MapDefinition;
using OSGeo.MapGuide.ObjectModels.TileSetDefinition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MgTileSeeder
{
    class CommonSeederOptions
    {
        // Making these virtual so that we can override and tack on [Option] in subclasses

        public virtual double MinX { get; set; }

        public virtual double MinY { get; set; }

        public virtual double MaxX { get; set; }

        public virtual double MaxY { get; set; }

        [Option("wait", Default = false)]
        public bool Wait { get; set; }

        public virtual void Validate() { }
    }

    [Verb("xyz", HelpText = "XYZ tiling options")]
    class XYZSeederOptions : CommonSeederOptions
    {
        [Option("url", Required = true, HelpText = "The URL of the XYZ tile source. It must have {x}, {y} and {z} placeholders")]
        public string UrlTemplate { get; set; }

        [Option("minx", SetName = "bbox", Required = true)]
        public override double MinX { get; set; }

        [Option("miny", SetName = "bbox", Required = true)]
        public override double MinY { get; set; }

        [Option("maxx", SetName = "bbox", Required = true)]
        public override double MaxX { get; set; }

        [Option("maxy", SetName = "bbox", Required = true)]
        public override double MaxY { get; set; }

        public override void Validate()
        {
            if (!Utility.InRange(this.MinX, -180, 180))
                throw new Exception("minx not in range of [-180, 180]");
            if (!Utility.InRange(this.MaxX, -180, 180))
                throw new Exception("maxx not in range of [-180, 180]");
            if (!Utility.InRange(this.MinY, -90, 90))
                throw new Exception("miny not in range of [-90, 90]");
            if (!Utility.InRange(this.MaxY, -90, 90))
                throw new Exception("maxy not in range of [-90, 90]");

            if (this.MinX > this.MaxX)
                throw new Exception("Invalid BBOX: minx > maxx");
            if (this.MinY > this.MaxY)
                throw new Exception("Invalid BBOX: miny > maxy");
        }
    }

    [Verb("mapguide", HelpText = "MapGuide tiling options")]
    class MgTileSeederOptions : CommonSeederOptions
    {
        [Option('m', "mapagent", Required = true, HelpText = "The mapagent endpoint URL")]
        public string MapAgentUri { get; set; }

        [Option("map", Required = true, HelpText = "The resource id of the tiled map definition or tile set definition to seed")]
        public string ResourceID { get; set; }

        [Option("groups", HelpText = "A list of base layer groups to seed. If not specified, all base layer groups of the specified map are seeded")]
        public IEnumerable<string> Groups { get; set; }

        [Option('u', "username", Default = "Anonymous", HelpText = "The MapGuide username")]
        public string Username { get; set; }

        [Option("password", Default = "", HelpText = "The password of the specified MapGuide user")]
        public string Password { get; set; }

        [Option("meters-per-unit", HelpText = "The meters per unit value. If connecting to a 2.6 or higher server and tiling a map definition, this value can be inferred automatically")]
        public double MetersPerUnit { get; set; }

        [Option("minx", SetName = "bbox")]
        public override double MinX { get; set; }

        [Option("miny", SetName = "bbox")]
        public override double MinY { get; set; }

        [Option("maxx", SetName = "bbox")]
        public override double MaxX { get; set; }

        [Option("maxy", SetName = "bbox")]
        public override double MaxY { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default
                  .ParseArguments<MgTileSeederOptions, XYZSeederOptions>(args)
                  .MapResult(opts => Run(opts), _ => 1);
        }

        class ConsoleProgress : IProgress<TileProgress>
        {
            public void Report(TileProgress value)
            {
                Console.WriteLine($"Rendered {value.Rendered} of {value.Total} tiles [{((double)value.Rendered / (double)value.Total):P}]");
            }
        }

        static int Run(object arg)
        {
            switch (arg)
            {
                case MgTileSeederOptions mgOpts:
                    {
                        int ret = RunMapGuide(mgOpts);
                        Environment.ExitCode = ret;
                        return ret;
                    }
                case XYZSeederOptions xyzOpts:
                    {
                        int ret = RunXYZ(xyzOpts);
                        Environment.ExitCode = ret;
                        return ret;
                    }
                default:
                    throw new ArgumentException();
            }
        }

        static int RunXYZ(XYZSeederOptions options)
        {
            int ret = 0;
            try
            {
                options.Validate();

                var xyz = new XYZTileService(options.UrlTemplate);
                var walker = new XYZTileWalker(options.MinX, options.MinY, options.MaxX, options.MaxY);

                var seederOptions = new TileSeederOptions();
                var seeder = new TileSeeder(xyz, walker, seederOptions);

                var progress = new ConsoleProgress();
                var stats = seeder.Run(progress);

                Console.WriteLine($"Rendered {stats.TilesRendered} tiles in {stats.Duration}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ret = 1;
            }
            finally
            {
                if (options.Wait)
                {
                    Console.WriteLine("Press any key to continue");
                    Console.Read();
                }
            }
            return ret;
        }

        static int RunMapGuide(MgTileSeederOptions options)
        {
            int ret = 0;
            try
            { 
                options.Validate();

                var conn = ConnectionProviderRegistry.CreateConnection("Maestro.Http", 
                    "Url", options.MapAgentUri, 
                    "Username", options.Username, 
                    "Password", options.Password);

                var tileSvc = (ITileService)conn.GetService((int)ServiceType.Tile);
                var res = conn.ResourceService.GetResource(options.ResourceID);
                DefaultTileWalkOptions walkOptions = null;
                switch (res)
                {
                    case IMapDefinition mdf:
                        walkOptions = new DefaultTileWalkOptions(mdf, options.Groups.ToArray());
                        //If meters-per-unit not specified and this is >= 2.6 or higher, we can use
                        //CREATERUNTIMEMAP to get this value
                        if (options.MetersPerUnit == default(double))
                        {
                            if (conn.SiteVersion >= new Version(2, 6))
                            {
                                Console.WriteLine("Using CREATERUNTIMEMAP to obtain required meters-per-unit value");
                                var createRt = (ICreateRuntimeMap)conn.CreateCommand((int)CommandType.CreateRuntimeMap);
                                createRt.MapDefinition = options.ResourceID;
                                createRt.RequestedFeatures = (int)RuntimeMapRequestedFeatures.None;
                                var rtMapInfo = createRt.Execute();
                                options.MetersPerUnit = rtMapInfo.CoordinateSystem.MetersPerUnit;
                                Console.WriteLine($"Using meters-per-unit value of: {options.MetersPerUnit}");
                            }
                        }
                        break;
                    case ITileSetDefinition tsd:
                        walkOptions = new DefaultTileWalkOptions(tsd, options.Groups.ToArray());
                        //If meters-per-unit not specified and the tile set is using the "Default" provider, we can create
                        //a Map Definition linked to the tile set, save it to a temp resource and call CREATERUNTIMEMAP
                        //from it to obtain the reuqired meters-per-unit value
                        if (options.MetersPerUnit == default(double) && tsd.TileStoreParameters.TileProvider == "Default")
                        {
                            IMapDefinition3 mdf3 = (IMapDefinition3)ObjectFactory.CreateMapDefinition(new Version(3, 0, 0), "LinkedTileSet");
                            string tmpId = $"Session:{conn.SessionID}//{mdf3.Name}.MapDefinition";
                            var text = tsd.Extents;
                            mdf3.SetExtents(text.MinX, text.MinY, text.MaxX, text.MaxY);
                            mdf3.CoordinateSystem = tsd.GetDefaultCoordinateSystem();
                            mdf3.TileSetDefinitionID = tsd.ResourceID;
                            conn.ResourceService.SaveResourceAs(mdf3, tmpId);

                            Console.WriteLine("Using CREATERUNTIMEMAP to obtain required meters-per-unit value");
                            var createRt = (ICreateRuntimeMap)conn.CreateCommand((int)CommandType.CreateRuntimeMap);
                            createRt.MapDefinition = tmpId;
                            createRt.RequestedFeatures = (int)RuntimeMapRequestedFeatures.None;
                            var rtMapInfo = createRt.Execute();
                            options.MetersPerUnit = rtMapInfo.CoordinateSystem.MetersPerUnit;
                            Console.WriteLine($"Using meters-per-unit value of: {options.MetersPerUnit}");
                        }
                        break;
                    default:
                        throw new ArgumentException("Invalid resource type");
                }

                walkOptions.MetersPerUnit = options.MetersPerUnit;
                var walker = new DefaultTileWalker(walkOptions);

                var seederOptions = new TileSeederOptions();
                var seeder = new TileSeeder(tileSvc, walker, seederOptions);

                var progress = new ConsoleProgress();
                var stats = seeder.Run(progress);

                Console.WriteLine($"Rendered {stats.TilesRendered} tiles in {stats.Duration}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ret = 1;
            }
            finally
            {
                if (options.Wait)
                {
                    Console.WriteLine("Press any key to continue");
                    Console.Read();
                }
            }
            return ret;
        }
    }
}