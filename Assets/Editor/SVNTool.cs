using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SVNTool : EditorWindow {
	private static List<string> assetPaths;
	private static Dictionary<string, bool> listItems;
	private static List<string> selectedAssetsPaths;

	private static string log;

	private static Vector2 scrollPos;

	public static void Init(List<string> paths = null){
		// 没有待操作的资源，不弹出窗口
		if(paths == null || paths.Count == 0){
			return;
		}

		assetPaths = paths;
		
		listItems = new Dictionary<string, bool>();
		// 初始化资源路径
		foreach (string path in assetPaths) {
			listItems.Add(path, true);
		}

		selectedAssetsPaths = new List<string>();

		SVNTool window = (SVNTool)EditorWindow.GetWindow(typeof(SVNTool), false, "SVN Tool");
		window.Show();
	}

	void OnGUI() {
		EditorGUILayout.BeginVertical();
		EditorGUILayout.LabelField("Log message:");
		log = EditorGUILayout.TextArea(log, GUILayout.Height(100));
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Assets path:");
		// 取消全部
		if(GUILayout.Button("None", GUILayout.Height(18), GUILayout.Width(60))) {
			UpdateSelection(false);
		}
		// 选择全部
		if(GUILayout.Button("All", GUILayout.Height(18), GUILayout.Width(60))) {
			UpdateSelection(true);
		}
		EditorGUILayout.EndHorizontal();

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true);
		foreach(string path in assetPaths){
			listItems[path] = EditorGUILayout.ToggleLeft(path, listItems[path], GUILayout.Height(18));
		}
		EditorGUILayout.EndScrollView();

		if(GUILayout.Button("Commit")) {

		}

		EditorGUILayout.EndVertical();
	}

	void OnInspectorUpdate() {
		Repaint();
	}

	private static void UpdateSelection(bool status){
		foreach(string path in assetPaths){
			listItems[path] = status;
		}
	}

	// [MenuItem("Assets/SVN Tool/SVN 更新")]  
	// private static void SvnToolUpdate() {     
	// 	List<string> assetPaths = SelectionUtil.GetSelectionAssetPaths();     
	// 	UpdateAtPaths(assetPaths);     
	// }


 	// [MenuItem("Assets/SVN Tool/SVN 提交...")] 
  	// private static void SvnToolCommit() {           
  	// 	List<string> assetPaths = SelectionUtil.GetSelectionAssetPaths();    
 	// 	CommitAtPaths(assetPaths);    
  	// }

	// [MenuItem("Assets/SVN Tool/显示日志")] 
	// private static void SvnToolLog() {        
	// 	List<string> assetPaths = SelectionUtil.GetSelectionAssetPaths();     
	// 	if (assetPaths.Count == 0) {            
	// 		return;          
	// 	}

	// 	// 显示日志，只能对单一资产    
	// 	string arg = "/command:log /closeonend:0 /path:\""; 
	// 	arg += assetPaths[0];         
	// 	arg += "\"";       
	// 	SvnCommandRun(arg);       
	// }

	// [MenuItem("Assets/SVN Tool/全部更新", false, 1100)] 
	// private static void SvnToolAllUpdate() {      
	// 	// 往上两级，包括数据配置文件     
	// 	string arg = "/command:update /closeonend:0 /path:\"";     
	// 	arg += ".";    
	// 	arg += "\"";       
	// 	SvnCommandRun(arg); 
	// }   


	// [MenuItem("Assets/SVN Tool/全部日志", false, 1101)] 
	// private static void SvnToolAllLog() {         
	// 	// 往上两级，包括数据配置文件      
	// 	string arg = "/command:log /closeonend:0 /path:\""; 
	// 	arg += ".";      
	// 	arg += "\"";         
	// 	SvnCommandRun(arg);
	// }      
 
	// [MenuItem("Assets/SVN Tool/全部恢复", false, 1102)] 
	// private static void SvnToolAllRevert() {         
	// 	// 往上两级，包括数据配置文件      
	// 	string arg = "/command:revert /closeonend:0 /path:\""; 
	// 	arg += ".";      
	// 	arg += "\"";         
	// 	SvnCommandRun(arg);   
	// }   
}
