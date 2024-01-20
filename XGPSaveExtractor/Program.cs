using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using ExtensionMethods;

namespace XGPSaveExtractor
{
    class XGPSaveExtractor
    {
        /*List<(string, string)> supported_xgp_apps = new List<(string, string)>
        {
            ("Yakuza 0", "SEGAofAmericaInc.Yakuza0PC_s751p9cej88mt"),
            ("Yakuza Like a Dragon", "SEGAofAmericaInc.Yazawa_s751p9cej88mt"),
            ("Octopath Traveller", "39EA002F.FrigateMS_n746a19ndrrjg"),
            ("Just Cause 4", "39C668CD.JustCause4-BaseGame_r7bfsmp40f67j"),
            ("Hades", "SupergiantGamesLLC.Hades_q53c1yqmx7pha"),
            ("Control", "505GAMESS.P.A.ControlPCGP_tefn33qh9azfc"),
            ("Atomic Heart", "FocusHomeInteractiveSA.579645D26CFD_4hny5m903y3g0"),
            ("Chorus", "DeepSilver.UnleashedGoF_hmv7qcest37me"),
            ("Final Fantasy XV", "39EA002F.FINALFANTASYXVforPC_n746a19ndrrjg"),
            ("Starfield", "BethesdaSoftworks.ProjectGold_3275kfvn8vcwc"),
            ("A Plague Tale: Requiem", "FocusHomeInteractiveSA.APlagueTaleRequiem-Windows_4hny5m903y3g0"),
            ("High on Life", "2637SquanchGamesInc.HighonLife_mh7dg3tfmz2cj"),
            ("Lies of P", "Neowiz.3616725F496B_r4z3116tdh636"),
            ("Totally Accurate Battle Simulator", "LandfallGames.TotallyAccurateBattleSimulator_r2vq7k2y0v9ct"),
            ("Celeste", "MattMakesGamesInc.Celeste_79daxvg0dq3v6"),
            ("Persona 5 Royal", "SEGAofAmericaInc.F0cb6b3aer_s751p9cej88mt"),
            ("Persona 5 Tactica", "SEGAofAmericaInc.s0cb6b3ael_s751p9cej88mt"),
            ("Chained Echoes", "DECK13.ChainedEchoesRelease_rn1dn9jh54zft"),
            ("Wo Long: Fallen Dynasty", "946B6A6E.WoLongFallenDynasty_dkffhzhmh6pmy")
        };*/

        readonly static string PALWORLD_PKG_NAME = "PocketpairInc.Palworld_ad4psfrxyesvt";

        static (string?, ContainerInfo[]) ReadUserContainers(string user_wgs_dir)
        {
            // def read_user_containers(user_wgs_dir: Path) -> Tuple[str, List[Dict[str, Any]]]:

            string containers_dir = user_wgs_dir;
            string containers_idx_path = Path.Combine(containers_dir, "containers.index");

            List<ContainerInfo> containers = new List<ContainerInfo>();
            string? store_pkg_name = null;

            // Read the index file
            using (FileStream f = File.OpenRead(containers_idx_path))
            {
                BinaryReader br = new BinaryReader(f);

                // Unknown
                br.BaseStream.Seek(4, SeekOrigin.Current);

                int container_count = br.ReadInt32();

                // Unknown
                br.BaseStream.Seek(4, SeekOrigin.Current);

                string store_pkg_name_string = br.ReadUTF16String();
                store_pkg_name = store_pkg_name_string.Split("!")[0];
                //Console.WriteLine($"Store pkg name: {store_pkg_name}");


                // Creation date, FILETIME
                DateTime creation_date = br.ReadFileTime();

                // Unknown
                br.BaseStream.Seek(4, SeekOrigin.Current);

                string unknown_guid_string = br.ReadUTF16String();

                // Unknown
                br.BaseStream.Seek(8, SeekOrigin.Current);

                for (int i = 0; i < container_count; i++)
                {
                    // Container name
                    string container_name = br.ReadUTF16String();
                    // Duplicate of the file name
                    string container_name_2 = br.ReadUTF16String();
                    // Unknown quoted hex number
                    string unknown_hex_string = br.ReadUTF16String();
                    // Container number
                    byte container_num = br.ReadByte();
                    // Unknown
                    br.BaseStream.Seek(4, SeekOrigin.Current);
                    // Read container (folder) GUID
                    Guid container_guid = new Guid(br.ReadBytes(16));
                    // Creation date, FILETIME
                    DateTime container_creation_date = br.ReadFileTime();
                    //Console.WriteLine($"\tContainer \"{container_name}\" created at {container_creation_date}");
                    // Unknown
                    br.BaseStream.Seek(16, SeekOrigin.Current);

                    List<FileInfo> files = new List<FileInfo>();

                    // Read the container file in the container directory
                    string container_path = Path.Combine(containers_dir, container_guid.ToString("N").ToUpper());
                    string container_file_path = Path.Combine(container_path, $"container.{container_num}");

                    if (!File.Exists(container_file_path))
                    {
                        Console.WriteLine($"Missing container \"{container_name}\"");
                        continue;
                    }

                    using (FileStream cf = File.OpenRead(container_file_path))
                    {
                        BinaryReader cbr = new BinaryReader(cf);
                        // Unknown (always 04 00 00 00 ?)
                        uint magic = cbr.ReadUInt32();
                        // Number of files in this container
                        int file_count = cbr.ReadInt32();

                        for (int j = 0; j < file_count; j++)
                        {
                            // File name, 0x80 (128) bytes UTF-16 = 64 characters
                            string file_name = cbr.ReadUTF16String(64).TrimEnd('\0');
                            // Read file GUID
                            Guid file_guid_1 = new Guid(cbr.ReadBytes(16));
                            // Read the copy of the GUID
                            Guid file_guid_2 = new Guid(cbr.ReadBytes(16));

                            string file_path;
                            Guid file_guid;

                            if (file_guid_1 == file_guid_2)
                            {
                                file_path = Path.Combine(container_path, file_guid_1.ToString("N").ToUpper());
                                file_guid = file_guid_1;
                            }
                            else
                            {
                                // Check if one of the file paths exist
                                string file_guid_1_path = Path.Combine(container_path, file_guid_1.ToString("N").ToUpper());
                                string file_guid_2_path = Path.Combine(container_path, file_guid_2.ToString("N").ToUpper());

                                bool file_1_exists = File.Exists(file_guid_1_path);
                                bool file_2_exists = File.Exists(file_guid_2_path);

                                if (file_1_exists && !file_2_exists)
                                {
                                    file_path = file_guid_1_path;
                                    file_guid = file_guid_1;
                                }
                                else if (!file_1_exists && file_2_exists)
                                {
                                    file_path = file_guid_2_path;
                                    file_guid = file_guid_2;
                                }
                                else if (file_1_exists && file_2_exists)
                                {
                                    // Which one to use?
                                    Console.WriteLine($"Two files exist for container \"{container_name}\" file \"{file_name}\": {file_guid_1} and {file_guid_2}, can't choose one");
                                    continue;
                                }
                                else
                                {
                                    Console.WriteLine($"Missing file \"{file_name}\" inside container \"{container_name}\"");
                                    continue;
                                }
                            }

                            files.Add(new FileInfo(file_name, file_guid, file_path));
                        }

                        containers.Add(new ContainerInfo(container_name, container_num, container_guid, files.ToArray()));
                    }
                }
            }

            return (store_pkg_name, containers.ToArray());
        }

        static int ExitMessage(string? message, int exitCode)
        {
            if (message != null)
            {
                Console.WriteLine(message);
            }

            Console.Write("\nPress any key to exit...");
            Console.ReadKey();

            return exitCode;
        }

        static int Main(string[] args)
        {
            Console.WriteLine("PalWorld XGP Save Extractor");

            Console.WriteLine("Detecting PalWorlds XGP Save Location...");
            string? localAppDataPath = Environment.GetEnvironmentVariable("LocalAppData");
            if (localAppDataPath == null)
            {
               return ExitMessage("Unable to expand %LocalAppData% environment variable", -1);
            }
            string savePath = Path.Combine(localAppDataPath, "Packages", PALWORLD_PKG_NAME, "SystemAppData\\wgs");

            if(!Directory.Exists(savePath))
            {
                return ExitMessage("Unable to detect PalWorlds save location", -2);
            }

            string[] dirs = Directory.GetDirectories(savePath, "*_*");
            if(dirs.Length == 0)
            {
                return ExitMessage("Unable to find active save folder", -3);
            }

            int selection = -1;
            if (dirs.Length > 1)
            {
                Console.WriteLine("\nSelect Save Folder to Extract:");
                for (int i = 0; i < dirs.Length; i++)
                {
                    Console.WriteLine($"[{i}] {Path.GetFileName(dirs[i])}");
                }
                Console.Write("\nEnter Selection: ");

                // Check valid selection
                string? selectString = Console.ReadLine();
                if (!int.TryParse(selectString, out selection))
                {
                    return ExitMessage("Failed to parse selection", -4);
                }

                if (selection < 0 || selection > (dirs.Length - 1))
                {
                    return ExitMessage("Selection out of bounds", -5);
                }
            }
            else
            {
                Console.WriteLine($"Only 1 save folder found, using {dirs[0]}");
                selection = 0;
            }

            // Read container structure, converted from https://github.com/Z1ni/XGP-save-extractor
            // All credits to Z1ni & snoozbuster for figuring out the container format at goatfungus/NMSSaveEditor#306.
            (string? pkg_name, ContainerInfo[] containers) = ReadUserContainers(dirs[selection]);
            if (pkg_name != "PocketpairInc.Palworld_ad4psfrxyesvt")
            {
                return ExitMessage("Not PalWorld Game", -6);
            }


            // each container is one file
            foreach (ContainerInfo container in containers)
            {
                string fileName = Path.Combine("temp", container.Name.Replace("-", "\\"));
                string fullFileName = Path.ChangeExtension(fileName, ".sav");
                string? basePath = Path.GetDirectoryName(fullFileName);

                // Create directory tree
                if (!Directory.Exists(basePath))
                {
                    if (basePath != null)
                    {
                        Console.WriteLine($"Creating temp directory {Path.GetFullPath(basePath)}");
                        Directory.CreateDirectory(basePath);
                    }
                }

                // Copy file
                FileInfo file = container.Files[0];
                File.Copy(file.Path, Path.ChangeExtension(fileName, ".sav"), true);
            }

            string archiveFolderName = "extracted";
            string archiveName = Path.Combine(archiveFolderName, $"extacted{DateTime.Now:yyyyMMddHHmmssffff}.zip");

            if (!Directory.Exists(archiveFolderName))
            {
                Console.WriteLine($"Creating archive directory {Path.GetFullPath(archiveFolderName)}");
                Directory.CreateDirectory(archiveFolderName);
            }

            Console.WriteLine($"Creating zip archive {Path.GetFullPath(archiveName)}");
            ZipFile.CreateFromDirectory("temp", archiveName);

            Console.WriteLine($"Deleting temp directory {Path.GetFullPath("temp")}");
            Directory.Delete("temp", true);

            return ExitMessage(null, 0);
        }
    }
}

namespace ExtensionMethods
{
    public static class BinaryReaderExtension
    {
        public static DateTime ReadFileTime(this BinaryReader br)
        {
            long filetime = br.ReadInt64();
            return DateTime.FromFileTime(filetime);
        }

        public static string ReadUTF16String(this BinaryReader br, int? len = null)
        {
            int stringLength;
            if (len.HasValue)
            {
                stringLength = (int)len;
            }
            else
            {
                stringLength = br.ReadInt32();
            }

            // TODO: Get MAX_PATH dynamically
            if (stringLength > 260 - 1)
            {
                return String.Empty;
            }

            byte[] byteString = br.ReadBytes(stringLength * 2);
            return Encoding.Unicode.GetString(byteString);
        }

    }
}