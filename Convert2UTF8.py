# -*- coding: cp936 -*-
import os
import chardet

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
    #Handle .lua File
    if item.endswith(".lua"):
        txtFile = open(item)
        all_the_text = txtFile.read()
        txtFile.close()
        result = chardet.detect(all_the_text)
        coding = result.get('encoding')
        #print result
        if coding is None or coding == "utf-8":
          return
        
        txtFile = open(item, "wt")
        txtFile.write(all_the_text.decode(coding).encode("utf-8"))
        txtFile.close()
        print(item, " --> OK("+coding+" > utf-8)",)
      
if __name__=="__main__":
    cur_Path = os.getcwd()
    getalldir(cur_Path)
    
raw_input('convertfile finished (to utf-8). . .')
