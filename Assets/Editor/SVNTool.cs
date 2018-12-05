using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Dependency{
	public class SVNTool : EditorWindow {
		private static SVNTool m_window = null;

		private static Dictionary<string, bool> m_folderListItems;
		private static List<string> m_filePaths;
		private static List<string> m_folderPaths;

		// commit时的log信息
		private static string m_log;

		// 用于绘制滑动框
		private static Vector2 m_folderScrollPos;
		private static Vector2 m_fileScrollPos;

		public static void Init(List<string> paths = null){
			// 没有待操作的资源，不弹出窗口
			if(paths == null || paths.Count == 0){
				return;
			}

			m_filePaths = paths;
			m_folderPaths = new List<string>();
			m_folderListItems = new Dictionary<string, bool>();

			foreach(var file in m_filePaths){
				AddFolder(file);
			}

			foreach(var folder in m_folderListItems.Keys){
				m_folderPaths.Add(folder);
			}

			m_window = (SVNTool)EditorWindow.GetWindow(typeof(SVNTool), false, "SVN Tool");
			m_window.minSize = new Vector2(600, 540);
			m_window.maximized = false;
			m_window.Show();
		}

		public static void CloseWindow(){
			if(m_window == null){
				return;
			}

			m_window.Close();
			m_window = null;
		}
		private void OnGUI() {
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Log message:");
			m_log = EditorGUILayout.TextArea(m_log, GUILayout.Height(100));

#region 文件夹选择框
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("The newly created folder must be selected to commit the selected files!");
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Folders: ");
			// 取消全部
			if(GUILayout.Button("None", GUILayout.Height(18), GUILayout.Width(60))) {
				foreach(string path in m_folderPaths){
					m_folderListItems[path] = false;
				}
			}
			// 选择全部
			if(GUILayout.Button("All", GUILayout.Height(18), GUILayout.Width(60))) {
				foreach(string path in m_folderPaths){
					m_folderListItems[path] = true;
				}
			}
			EditorGUILayout.EndHorizontal();
			m_folderScrollPos = EditorGUILayout.BeginScrollView(m_folderScrollPos, false, true);
			foreach(string path in m_folderPaths){
				m_folderListItems[path] = EditorGUILayout.ToggleLeft(path, m_folderListItems[path], GUILayout.Height(18));
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
#endregion

#region 文件选择框
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Selected files:");
			EditorGUILayout.EndHorizontal();
			m_fileScrollPos = EditorGUILayout.BeginScrollView(m_fileScrollPos, false, true);
			foreach(string path in m_filePaths){
				EditorGUILayout.SelectableLabel(path, GUILayout.Height(18));
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
#endregion

			if(GUILayout.Button("Commit")) {
				List<string> files = new List<string>();
				List<string> folders = new List<string>();

				foreach (var file in m_filePaths) {
					files.Add(file);
					// 添加meta文件
					if(File.Exists(file + ".meta")){
						folders.Add(file + ".meta");
					}
				}
				foreach(var folder in m_folderListItems.Keys){
					if(m_folderListItems[folder]) {
						folders.Add(folder);
						// 添加meta文件
						if(File.Exists(folder + ".meta")){
							folders.Add(folder + ".meta");
						}
					}
				}
				
				if(folders.Count == 0){
					// 没有选择任何文件夹
					SVNUtility.CommitAtPaths(files, m_log);
				}else{
					// 选择了文件夹
					SVNUtility.CommitFolderAndFile(files, folders, m_log);
				}
			}

			EditorGUILayout.EndVertical();
		}

		void OnInspectorUpdate() {
			this.Repaint();
		}

		private static void AddFolder(string folder) {
			int index = folder.LastIndexOf('/');
			if(index == -1) {
				return;
			} else {
				string subFolder = folder.Substring(0, index);
				AddFolder(subFolder);
				if(!m_folderListItems.ContainsKey(subFolder)) {
					m_folderListItems.Add(subFolder, false);
				}
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
}
