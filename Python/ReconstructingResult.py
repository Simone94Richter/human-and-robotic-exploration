import json
import os
#import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/Unity/Project Arena/Assets/Results"
inputDir2 = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/Python"
fileName = "resultPositionNum.json"
fileName2 = "MapToRead.txt"
#maxlen = 0

def rotate(x,y, origin=(0,0)):
    # shift to origin
    x1 = x #- origin[0]
    y1 = y #- origin[1]

    #rotate
    x2 = y1
    y2 = -x1

    if os.path.isfile(inputDir + "/" + fileName):
        with open(inputDir + "/" + fileName) as json_file:
            data = json.load(json_file)
            array_name = data['mapName']
            if(array_name[0] == "uffici2.map"):
                # shift back
                x3 = x2
                y3 = y2 +53
            if(array_name[0] == "open2.map"):
                # shift back
                x3 = x2
                y3 = y2 +57
            if(array_name[0] == "open1.map"):
                # shift back
                x3 = x2
                y3 = y2 +48 

    return x3, y3

if os.path.isfile(inputDir + "/" + fileName) and os.path.isfile(inputDir2 + "/" + fileName2):
    with open(inputDir2 + "/" + fileName2) as f:
        array = []
        content = f.readlines()

        content = [x.strip() for x in content]
        #maxlen = len(content)
        #array_unknown = list()
        #array_clear = list()
        #array_wall = list()
        #array_goal = list()
        j = 0
        for line in content:
            #print(line)
            #array.append(line)
            array = line.split(',')
            #print(line)
            #print(len(array))
            for i in range(len(array)):
                a = int(array[i])
                if(a == 1):
                    plt.plot(i, len(content)-j, 'ks')
                
                if(a == 4):
                    plt.plot(i, len(content)-j , 'gs')

            j = j + 1    

    with open(inputDir + "/" + fileName) as json_file:
        data = json.load(json_file)
        array_pos = data['position']
        #fig, (ax1, ax2) = plt.subplots(1,2, figsize=(7,3.3))

        for k in range(len(array_pos)):
            for s in array_pos[k].split():
                x,z = s.split(",")
                origin = (0.0,0.0)
                x, z = rotate(int(x),int(z), origin )
                #x = maxlen-int(x)
                #z = int(z)
                #print(x, z)
                if k+1 < len(array_pos):
                    a,b = array_pos[k+1].split(",")
                    a, b = rotate(int(a),int(b), origin )
                    #a = int(a)
                    #b = int(b)
                    plt.plot([x, a], [z, b], 'k-')

    #plt.axis([0, 50, 0, 50])
    plt.show()






