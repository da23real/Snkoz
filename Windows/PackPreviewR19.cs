using System;using System.IO;using System.Linq;using System.Collections.Generic;
namespace Synkoz {public sealed partial class AnimationPickerA01 {
 readonly Dictionary<string,string> representativesR19=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
 string RepresentativeR19(string folder){lock(representativesR19){string found;if(representativesR19.TryGetValue(folder,out found))return found;found=FindRepresentativeR19(folder,new HashSet<string>(StringComparer.OrdinalIgnoreCase));representativesR19[folder]=found;return found;}}
 internal static string FindRepresentativeR19(string folder,HashSet<string> visited){try{if(!Directory.Exists(folder)||(File.GetAttributes(folder)&FileAttributes.ReparsePoint)!=0||!visited.Add(Path.GetFullPath(folder)))return null;var file=Directory.GetFiles(folder).Where(TagIndexR19.IsAnimation).OrderBy(x=>x,StringComparer.OrdinalIgnoreCase).FirstOrDefault();if(file!=null)return file;foreach(var dir in Directory.GetDirectories(folder).OrderBy(x=>x,StringComparer.OrdinalIgnoreCase)){var result=FindRepresentativeR19(dir,visited);if(result!=null)return result;}}catch{}return null;}
}}
