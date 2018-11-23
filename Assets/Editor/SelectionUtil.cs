using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectionUtil {
	/// <summary>  
	/// 在Project面板中选中资产路径列表    
	/// </summary>   
	/// <returns></returns> 
	public static List<string> GetSelectionAssetPaths() {       
		List<string> assetPaths = new List<string>();  
		// 这个接口才能取到两列模式时候的文件夹  
		foreach (var guid in Selection.assetGUIDs) {       
			if (string.IsNullOrEmpty(guid)) {         
				continue;        
			}          
			string path = AssetDatabase.GUIDToAssetPath(guid);    
			if (!string.IsNullOrEmpty(path)) {          
				assetPaths.Add(path);      
			}       
		} 
		return assetPaths;  
	}
}
