using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SVNUtility {
	/// <summary>  
  	/// 运行SVN命令     
  	/// </summary>       
  	/// <param name="arg"></param>
	private static void SvnCommandRun(string arg) {          
  		// string workDirectory = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal)); 
		string workDirectory = System.Environment.CurrentDirectory;
  		System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo {
					UseShellExecute = false,			// 设置为false，process将会直接通过可执行文件来创建，而不是通过shell来创建
					CreateNoWindow = true, 				// 设置为true，process开始的时候不会创建窗口
					FileName = "TortoiseProc", 			// 当UseShellExecute为false的时候，这里只能设置用于创建process的可执行文件
					Arguments = arg,					// 传递给FileName设置的可执行文件执行的命令				
					WorkingDirectory = workDirectory	// 当UseShellExecute为false的时候，WorkingDirectory指process开始的目录
				});
	} 

	/// <summary>        
	/// SVN更新指定的单个文件        
	/// 路径示例：Assets/1.png        
	/// </summary>        
	/// <param name="assetPaths"></param>        
	public static void UpdateAtPath(string assetPath) {            
		List<string> assetPaths = new List<string>();          
		assetPaths.Add(assetPath);            
		UpdateAtPaths(assetPaths);        
	}

	/// <summary>       
	/// SVN更新指定的多个文件
	/// 路径示例：Assets/1.png        
	/// </summary>        
	/// <param name="assetPaths"></param>       
	public static void UpdateAtPaths(List<string> assetPaths) {            
		// 没有需要update的资源，不执行任何操作
		if (assetPaths.Count == 0) {                
			return;           
		}

		// 组装提交命令
		string arg = "/command:update /closeonend:0 /path:\""; 
		for (int i = 0; i < assetPaths.Count; i++) {               
			var assetPath = assetPaths[i];           
			if (i != 0) {                
				arg += "*";        
			}                
			arg += assetPath;          
		}           
		arg += "\"";

		// 执行指令        
		SvnCommandRun(arg);
	}

	/// <summary>        
	/// SVN提交指定的路径     
	/// 路径示例：Assets/1.png    
	/// </summary>     
	/// <param name="assetPaths"></param>      
	public static void CommitAtPaths(List<string> assetPaths, string logmsg = null) {            
		// 没有需要commit的资源，不执行任何操作
		if (assetPaths.Count == 0) {              
			return;        
		}          
		string arg = "/command:commit /closeonend:0 /path:\"";      
		for (int i = 0; i < assetPaths.Count; i++) {             
			var assetPath = assetPaths[i];             
			if (i != 0) {                  
				arg += "*";          
			}             
			arg += assetPath;         
		}         
		arg += "\"";
		
		// 附加log信息
		if (!string.IsNullOrEmpty(logmsg)) {              
			arg += " /logmsg:\"" + logmsg + "\"";   
		}            

		// 执行指令
		SvnCommandRun(arg);      
	}
}
