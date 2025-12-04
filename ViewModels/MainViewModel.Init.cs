using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stand7
{
	public partial class MainViewModel
	{

		private void SetArchiveDirs()
		{
			var archivePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Archive");
			DirsCreate(archivePath);
			var subPath = System.IO.Path.Combine(archivePath, "Logs");
			DirsCreate(subPath);
			subPath = System.IO.Path.Combine(archivePath, "Reports");
			DirsCreate(subPath);
			subPath = System.IO.Path.Combine(archivePath, "Data");
			DirsCreate(subPath);
		}
		private void DirsCreate(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}
}
