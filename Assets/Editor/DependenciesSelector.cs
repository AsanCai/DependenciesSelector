using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Dependency{
    public class DependenciesSelector : EditorWindow {
        // 上一个选中的prefab对象
        private static Object m_preObj;
        // 当前选中的prefab对象
        private static Object m_obj;
        // obj的所有依赖对象
        private static List<Object> m_selectedObjs = new List<Object>();

        private static Vector2 m_scrollPos;
        // private static bool m_addFolder;

        // 等待复制的文件个数
        private static int m_remainCount;

        // 保存项目的绝对路径        
        private static string PROJECTDIR = System.Environment.CurrentDirectory.Replace('\\', '/');
        
        // 保存所有依赖资源的路径
        private static Dictionary<string, bool> m_originalPaths = new Dictionary<string, bool>();
        // m_originalPaths的keys
        private static List<string> m_dependentPaths = new List<string>();

        [MenuItem("Assets/Find Prefab Dependencies")]
        private static void FindPrefabDependencies(){
            m_preObj = null;
            m_obj = null;

            if(Selection.activeObject == null){
                return;
            }

            if(PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None) {
                m_obj = (Selection.activeObject) as GameObject;
            }

            Init();
        }

        private static void Init() {
            DependenciesSelector window = (DependenciesSelector)EditorWindow.GetWindow(typeof(DependenciesSelector), false, "Dependencies");
            window.Show();

            window.minSize = new Vector2(480, 300);
            window.maximized = false;
        }

        private void OnGUI() {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Prefab：", GUILayout.Width(75));
                m_obj = EditorGUILayout.ObjectField(m_obj, typeof(Object), false);
                if(m_preObj != m_obj) {
                    // 清除依赖文件信息
                    m_dependentPaths.Clear();
                    // 关闭Commit窗口
                    SVNTool.CloseWindow();                        
                }
                m_preObj = m_obj;
            EditorGUILayout.EndHorizontal();
            
            // 显示提示
            EditorGUILayout.BeginHorizontal();
            if (m_obj){
                if (GUILayout.Button("Check Dependencies")){
                    GetAllDependencies(m_obj);
                }
            }
            else{
#if UNITY_5_3_OR_NEWER
                EditorGUILayout.LabelField("Select an prefab first!", EditorStyles.centeredGreyMiniLabel);
#else
                EditorGUILayout.LabelField("Select an prefab first!");
#endif
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            // 使用ScollView显示所有资源路径
            if(m_dependentPaths.Count > 0) {
                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Dependent resource path:");

                // 全选
                if(GUILayout.Button("ALL", GUILayout.Height(18), GUILayout.Width(50))) {
                    foreach(string key in m_dependentPaths){
                        m_originalPaths[key] = true;
                    }
                }
                // 全不选
                if(GUILayout.Button("NONE", GUILayout.Height(18), GUILayout.Width(50))) {
                    foreach(string key in m_dependentPaths){
                        m_originalPaths[key] = false;
                    }
                }
                EditorGUILayout.EndHorizontal();

                m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos, false, true);
                foreach(string path in m_dependentPaths){
                    m_originalPaths[path] = EditorGUILayout.ToggleLeft(path, m_originalPaths[path], GUILayout.Height(18));
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Copy Resources")) {
                    string folder = EditorUtility.OpenFolderPanel("Folder to save dependencies", PROJECTDIR + "/Temp", "" );
                    if(!string.IsNullOrEmpty(folder)){
                        string dialog = "";
                        if(folder.Equals(PROJECTDIR)) {
                            dialog = "Choose the root folder of project will overwrite the original resources! Are you sure to continue?";
                        } else {
                            dialog = "Are you sure you want to copy all selected resources to the selected folder?\n" + folder;
                        }

                        // 弹出对话框进行确认
                        if(EditorUtility.DisplayDialog("Copy Resource to selected folder?", dialog, "Yes", "No")){
                            CopyAllDependencies(folder);
                        }
                    }
                }

                if(GUILayout.Button("Commit")){
                    SVNTool.Init(GetSelectedPaths(false));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.EndVertical();
        }

        private void OnInspectorUpdate() {
            this.Repaint();
        }


        /// <summary>
        /// 获取所有引用资源的路径
        /// </summary>     
        /// <param name="prefab">目标操作对象</param>  
        private static void GetAllDependencies(Object prefab){
            // 重置所有参数
            m_originalPaths.Clear();
            m_dependentPaths.Clear();
            m_selectedObjs.Clear();

            string currentPath = AssetDatabase.GetAssetPath(prefab);
            Object[] roots = new Object[] { prefab };

            // 添加prefab所在路径
            m_dependentPaths.Add(currentPath);
            m_selectedObjs.Add(prefab);

            Object[] dependentObjs = EditorUtility.CollectDependencies(roots);
            string path;
            foreach(Object dependency in dependentObjs){
                path = AssetDatabase.GetAssetPath(dependency);
                
                if(path == currentPath){
                    // 如果是自身，则不添加
                    continue;
                }

                // 因为EditorUtility.CollectDependencies有可能返回默认的Unity内置资源
                // 因此需要根据返回形式来判断当前引用是否为默认资源
                if(File.Exists(path)){
                    m_dependentPaths.Add(path);
                    m_selectedObjs.Add(dependency);
                }
            }

            foreach (var dependentPath in m_dependentPaths) {
                m_originalPaths.Add(dependentPath, true);
            }
        }

        /// <summary>
        /// 复制资源到指定文件夹
        /// </summary>     
        /// <param name="copyFolder">目标文件夹</param>  
        private static void CopyAllDependencies(string copyFolder){
            // 重置需要复制的文件数
            m_remainCount = 0;

            foreach(string path in m_dependentPaths) {
                if(m_originalPaths[path] == true){
                    // 计算需要复制的文件数
                    m_remainCount++;
                }
            }


            // 开始复制资源操作
            string sourPath, destPath;
            string destDirectory;
            try {
                // 如果选择的文件夹不存在，那么就创建文件夹
                if(!Directory.Exists(copyFolder)){
                    Directory.CreateDirectory(copyFolder);
                }

                foreach(string path in GetSelectedPaths(true)){
                    sourPath = PROJECTDIR + "/" + path;
                    destPath = copyFolder + "/" + path;
                    destDirectory = Path.GetDirectoryName(destPath);
                    
                    // 确保目标路径文件夹存在
                    if(!Directory.Exists(destDirectory)){
                        Directory.CreateDirectory(destDirectory);
                    }

                    // 采用强制覆盖的形式拷贝资源
                    File.Copy(sourPath, destPath, true);
                    m_remainCount --;

                    // 显示进度条
                    EditorUtility.DisplayProgressBar("Copy progress", "Remians " + m_remainCount + " files haven't been copyed.", 1.0f * m_remainCount / m_dependentPaths.Count);
                }
            } catch (System.Exception e) {
                EditorUtility.DisplayDialog("Copy faild", e.Message, "OK", "");
                // 拷贝资源失败
                return;
            }

            // 关闭进度条
            EditorUtility.ClearProgressBar();
        }


        /// <summary>
        /// 获得所有选中的文件路径
        /// </summary>     
        /// <param name="addMetaFile">是否添加文件对应的meta文件</param>      
        private static List<string> GetSelectedPaths(bool addMetaFile = false) {
            List<string> selectedPaths = new List<string>();

            foreach(string path in m_dependentPaths) {
                if(m_originalPaths[path] == true){
                    // 添加源文件
                    selectedPaths.Add(path);

                    if(addMetaFile) {
                        if(File.Exists(path + ".meta")) {
                            selectedPaths.Add(path + ".meta");
                        }
                    }
                }
            }

            return selectedPaths;
        }
    }
}