﻿using System;
using System.Collections.Generic;
using System.Text;

using Studyzy.IMEWLConverter.Generaters;
using Studyzy.IMEWLConverter.Helpers;

namespace Studyzy.IMEWLConverter.Entities
{
    /// <summary>
    /// 根据设置的样式，解析和展示词条
    /// </summary>
    public class ParsePattern
    {
        private readonly WordLibrary sample;

        public ParsePattern()
        {
            Sort = new List<int> { 1, 2, 3 };
            sample = new WordLibrary();
            sample.Count = 1234;
            sample.Word = "深蓝词库转换";
            sample.PinYin = new[] { "shen", "lan", "ci", "ku", "zhuan", "huan" };
            IsPinyinFormat = true;
        }
    
        private SelfDefiningCodeGenerater selfFactory = new SelfDefiningCodeGenerater();
        private bool isPinyinFormat = true;

        /// <summary>
        /// 是否是拼音模式的一个字一个音
        /// </summary>
        public bool IsPinyinFormat
        {
            get { return isPinyinFormat; }
            set
            {
                isPinyinFormat = value;
            }
        }
        /// <summary>
        /// 打开或保存自定义编码的文件时，使用的编码格式
        /// </summary>
        public Encoding TextEncoding { get; set; }

        private bool isPinyin = true;
        /// <summary>
        /// 是否是拼音编码，如果是，可以对汉字注音
        /// </summary>
        public bool IsPinyin
        {
            get { return isPinyin; }
            set { isPinyin = value; }
        }

        /// <summary>
        /// 对于多个字的编码的设定
        /// </summary>
        public string MutiWordCodeFormat { get; set; }
        /// <summary>
        /// 是否包含编码
        /// </summary>
        public bool ContainCode { get; set; }
        /// <summary>
        /// 是否包含词频
        /// </summary>
        public bool ContainRank { get; set; }
        /// <summary>
        /// 编码之间的分隔符
        /// </summary>
        public string CodeSplitString { get; set; }
        /// <summary>
        /// 编码、词频、汉字之间的分隔符
        /// </summary>
        public string SplitString { get; set; }
        /// <summary>
        /// 编码的分隔符所在位置
        /// </summary>
        public BuildType CodeSplitType { get; set; }
        /// <summary>
        /// 编码\汉字\词频的排序方式
        /// </summary>
        public List<int> Sort { get; set; }
        /// <summary>
        /// 每个字对应的编码的文件路径
        /// </summary>
        public string MappingTablePath { get; set; }
        /// <summary>
        /// 每个字对应的编码的字典
        /// </summary>
        public IDictionary<char,string> MappingTable { get; set; }
        #region Example
        
        /// <summary>
        /// 按照指定规则，生成一个示例
        /// </summary>
        /// <returns></returns>
        public string BuildWLStringSample()
        {

            IDictionary<char, string> dic = new Dictionary<char, string>()
                {
                    {'深', "shen"},
                    {'蓝', "lan"},
                    {'词', "ci"},
                    {'库', "ku"},
                    {'转', "zhuan"},
                    {'换', "huan"}
                };
           if (!IsPinyin)
           {
               if (!string.IsNullOrEmpty(MappingTablePath))
               {
                   dic = UserCodingHelper.GetCodingDict(MappingTablePath);
               }
           }
            string word = "";
            string result = "";

            foreach (var c in sample.Word)
            {
                word += c;
             
                result += BuildWlString(dic,1234,word) + "\r\n";
            }

            return result;
        }
        /// <summary>
        /// 传入一个字与码的集合，以及词频，根据用户设定的格式，生成一条词条字符串
        /// </summary>
        /// <param name="charCodes"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        private string BuildWlString(IDictionary<char, string> charCodes, int rank, string word = "")
        {
            string code = "";
            if (word == "")
            {
                foreach (var c in charCodes.Keys)
                {
                    word += c;
                }
            }
            if (ContainCode)
            {
                if (IsPinyinFormat)
                {
                    code = CollectionHelper.GetString(GetSelectWordCodes(word, charCodes), CodeSplitString, CodeSplitType);
                }
                else//多字一码，根据用户设置的编码规则，生成编码
                {
                    selfFactory.MutiWordCodeFormat = MutiWordCodeFormat;
                    selfFactory.MappingDictionary = charCodes;

                    code = selfFactory.GetCodeOfString(word)[0];
                }
            }
            return BuildWlString(word, code, rank);

        }
        #endregion

        #region 生成指定格式的字符串

        /// <summary>
        /// 只有单词的情况下，根据规则生成目标格式的编码
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        //public string BuildWlString(string word)
        //{
        //    //WordLibrary wl=new WordLibrary(){Word = word};
        //    return BuildWlString(word,null,1);
          
        //}

      
        /// <summary>
        /// 根据Pattern设置，将WordLibrary转换成该格式的字符串
        /// </summary>
        /// <param name="wl"></param>
        /// <returns></returns>
        public string BuildWlString(WordLibrary wl)
        {
            string code = "";
            if (ContainCode)
            {
                if (IsPinyinFormat)
                {
                    code = CollectionHelper.GetString(CollectionHelper.DescarteIndex1(wl.Codes), CodeSplitString, CodeSplitType);
                }
                else//多字一码，根据用户设置的编码规则，生成编码
                {
                    code = wl.SingleCode;
                }
            }
            return BuildWlString(wl.Word, code, wl.Count);


        }

        /// <summary>
        /// 根据Pattern设置，将汉字、编码、词频组合成设置的格式
        /// </summary>
        /// <param name="word"></param>
        /// <param name="code"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public string BuildWlString(string word, string code, int rank)
        {
          
            string cp = "";
            if (!ContainCode)
            {
                code = "";
            }
            //else if(string.IsNullOrEmpty(code))//没有Code就生成Code
            //{
            //    GenerateCode(word)
            //}
            var sb = new StringBuilder();
            if (ContainRank)
            {
                cp = rank.ToString();
            }
            var dic = new Dictionary<int, string>();
            dic.Add(Sort[0], code);
            dic.Add(Sort[1], word);
            dic.Add(Sort[2], cp);
            var newSort = new List<int>(Sort);
            newSort.Sort();
            foreach (int x in newSort)
            {
                if (dic[x] != "")
                {
                    sb.Append(dic[x] + SplitString);
                }
            }
            string str = sb.ToString();
            return str.Substring(0, str.LastIndexOf(SplitString));
        }
     
      
      
       

        #endregion
      

        #region 根据字符串生成WL
       
        /// <summary>
        /// 根据Pattern设置的格式，对输入的一行该格式的字符串转换成WordLibrary
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public WordLibrary BuildWordLibrary(string line)
        {
            var wl = new WordLibrary();
            string[] strlist = line.Split(new[] { SplitString }, StringSplitOptions.RemoveEmptyEntries);
            var newSort = new List<int>(Sort);
            newSort.Sort();
            if (isPinyin)
            {
                int index1 = Sort.FindIndex(i => i == newSort[0]); //最小的一个
                if (index1 == 0 && ContainCode) //第一个是编码
                {
                    wl.PinYinString = strlist[0];
                }
                if (index1 == 1)//第一个是汉字
                {
                    wl.Word = strlist[0];
                }
                if (index1 == 2 && ContainRank)//第一个是词频
                {
                    wl.Count = Convert.ToInt32(strlist[0]);
                }
                if (strlist.Length > 1)
                {
                    int index2 = Sort.FindIndex(i => i == newSort[1]); //中间的一个
                    if (index2 == 0 && ContainCode) //一个是Code
                    {
                        wl.PinYinString = strlist[1];
                    }
                    if (index2 == 1)
                    {
                        wl.Word = strlist[1];
                    }
                    if (index2 == 2 && ContainRank)
                    {
                        wl.Count = Convert.ToInt32(strlist[1]);
                    }
                }
                if (strlist.Length > 2)
                {
                    int index2 = Sort.FindIndex(i => i == newSort[2]); //最大的一个
                    if (index2 == 0 && ContainCode) //第一个是拼音
                    {
                        wl.PinYinString = strlist[2];
                    }
                    if (index2 == 1)
                    {
                        wl.Word = strlist[2];
                    }
                    if (index2 == 2 && ContainRank)
                    {
                        wl.Count = Convert.ToInt32(strlist[2]);
                    }
                }

                wl.PinYin = wl.PinYinString.Split(new[] { CodeSplitString }, StringSplitOptions.RemoveEmptyEntries);
            
            }
            else//不是拼音，那么就抛弃直接加入Unknown Code。
            {
                int index1 = Sort.FindIndex(i => i == newSort[0]); //最小的一个
                if (index1 == 0 && ContainCode) //第一个是Code
                {
                    wl.SetCode(CodeType.Unknown, strlist[0]);
                }
                if (index1 == 1)
                {
                    wl.Word = strlist[0];
                }
                if (index1 == 2 && ContainRank)
                {
                    wl.Count = Convert.ToInt32(strlist[0]);
                }
                if (strlist.Length > 1)
                {
                    int index2 = Sort.FindIndex(i => i == newSort[1]); //中间的一个
                    if (index2 == 0 && ContainCode) //第一个是Code
                    {
                        wl.SetCode(CodeType.Unknown, strlist[1]);
                    }
                    if (index2 == 1)
                    {
                        wl.Word = strlist[1];
                    }
                    if (index2 == 2 && ContainRank)
                    {
                        wl.Count = Convert.ToInt32(strlist[1]);
                    }
                }
                if (strlist.Length > 2)
                {
                    int index2 = Sort.FindIndex(i => i == newSort[2]); //最大的一个
                    if (index2 == 0 && ContainCode) //第一个是拼音
                    {
                        wl.SetCode(CodeType.Unknown, strlist[2]);
                    }
                    if (index2 == 1)
                    {
                        wl.Word = strlist[2];
                    }
                    if (index2 == 2 && ContainRank)
                    {
                        wl.Count = Convert.ToInt32(strlist[2]);
                    }
                }

            }
            return wl;
        }
        #endregion
        //public void CodingString(WordLibrary wl, IWordCodeGenerater factory)
        //{
        //    var codes = new List<string>();
        //    foreach (char c in wl.Word)
        //    {
        //        string code = factory.GetDefaultCodeOfChar(c);
        //        codes.Add(code);
        //    }
        //    wl.PinYin = codes.ToArray();
        //}

        private IEnumerable<string> GetSelectWordCodes(string word, IDictionary<char, string> charCodes)
        {
            if (word == "")
            {
                return charCodes.Values;
            }
            var result = new List<string>();
            foreach (var c in word)
            {
                result.Add(charCodes[c]);
            }
            return result;
        }
    }
}