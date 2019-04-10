# -*- coding: cp936 -*-
import os

def getalldir(filepath):
#遍历filepath下所有文件，包括子目录
  files = os.listdir(filepath)
  for fi in files:
    fi_d = os.path.join(filepath,fi)            
    if os.path.isdir(fi_d):
        getalldir(fi_d)                  
    else:
        convertfile(fi_d)

def convertfile(item):
    #Handle .cs File
    if item.endswith(".cs"):
        txtFile = open(item)
        all_the_text = txtFile.read()
        txtFile.close()
        txtFile = open(item, "wt")
        txtFile.write(all_the_text)
        txtFile.close()
        print(item, " --> OK")
      
if __name__=="__main__":
    cur_Path = os.getcwd()
    getalldir(cur_Path)
    
raw_input('请按任意键继续. . .')
