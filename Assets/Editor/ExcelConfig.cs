using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.IO;
using Excel;
using UnityEditor;

public class ExcelConfig
{
    public static readonly string excelFolderPath = Application.dataPath + "/Tables/";
    public static readonly string assetPath = "Assets/Resources/DataAssets/";

    public class ExcelTool
    {
        public static Lines[] ImportLinesWithExcel(string filePath)
        {
            int columnNum = 0, rowNum = 0;
            DataRowCollection collect = ReadExcel(filePath, ref columnNum, ref rowNum);

            //the first line in excel makes no sence
            Lines[] lines = new Lines[rowNum - 1];
            for (int i = 1; i < rowNum; i++)
            {
                Lines line = new Lines();
                //read line info
                line.plotID = int.Parse(collect[i][0].ToString());
                line.index = int.Parse(collect[i][1].ToString());
                line.nextIndex = int.Parse(collect[i][2].ToString());
                line.text = collect[i][3].ToString();
                line.duration = float.Parse(collect[i][4].ToString());
                line.name = collect[i][5].ToString();
                lines[i - 1] = line;
            }

            return lines;
        }


        static DataRowCollection ReadExcel(string filePath, ref int columnNum, ref int rowNum)
        {
            FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

            DataSet result = excelReader.AsDataSet();
            //read first table
            columnNum = result.Tables[0].Columns.Count;
            rowNum = result.Tables[0].Rows.Count;
            return result.Tables[0].Rows;
        }
    }


    public class ExcelBuild : Editor
    {

        [MenuItem("CustomEditor/CreateLinesAsset")]
        public static void CreateLinesAsset()
        {
            PlotManager manager = ScriptableObject.CreateInstance<PlotManager>();

            manager.lines = ExcelTool.ImportLinesWithExcel(ExcelConfig.excelFolderPath + "Plot.xlsx");

            if (!Directory.Exists(ExcelConfig.assetPath))
            {
                Directory.CreateDirectory(ExcelConfig.assetPath);
            }

            //asset file should be named with "Assets/..." in the beginning
            string assetPath = string.Format("{0}{1}.asset", ExcelConfig.assetPath, "Lines");
            //generate asset file
            AssetDatabase.CreateAsset(manager, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}


