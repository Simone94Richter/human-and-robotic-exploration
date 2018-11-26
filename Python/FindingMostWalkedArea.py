import json
import os
#import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/DownloadedResults"
inputDir2 = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/Python"
fileName = "Result"
fileName2 = ""
index = 1
mappa = input("Inserire il numero della mappa sulla quale lavorare (1 per uffici2, 2 per uffici1, 3 per open1 e 4 per open2): ")
#print(mappa)
finish = False
if(mappa == str(1)):
    map_name = "uffici2.map"
    fileName2 = "uffici2PythonFormat.map.txt"
if(mappa == str(2)):
    map_name = "uffici1.map"
    fileName2 = "uffici1PythonFormat.map.txt"
if(mappa == str(3)):
    map_name = "open1.map"
    fileName2 = "open1PythonFormat.map.txt"
if(mappa == str(4)):
    map_name = "open2.map"
    fileName2 = "open2PythonFormat.map.txt"
print(map_name)

dictionary_positions = {}

def rotate(x,y, origin=(0,0)):
    # shift to origin
    x1 = x #- origin[0]
    y1 = y #- origin[1]

    #rotate
    x2 = y1
    y2 = -x1

    if(map_name == "uffici2.map"):
        x3 = x2
        y3 = y2 +53
    if(map_name == "open2.map"):
        x3 = x2
        y3 = y2 +57
    if(map_name == "open1.map"):
        x3 = x2
        y3 = y2 +48
    if(map_name == "uffici1.map"):
        x3 = x2
        y3 = y2 +54 

    return x3, y3

while(finish == False):
    if os.path.isfile(inputDir + "/" + fileName + str(index) + "t.txt"):
        #print('Here')
        with open(inputDir + "/" + fileName + str(index) + "t.txt") as json_file:
            data = json.load(json_file)
            if(data['mapName'][0] == map_name):
                array_pos = data['position']
                for k in range(len(array_pos)):
                    for s in array_pos[k].split():
                        #print(s)
                        #x,z = s.split(",")
                        #origin = (0.0,0.0)
                        #x, z = rotate(int(x),int(z), origin )

                        if s in dictionary_positions:
                            count = dictionary_positions[s]
                            count = count + 1
                            dictionary_positions[s] = count
                        else:
                            dictionary_positions[s] = 1

                
            index = index + 1

    else:
        print('End')
        finish = True
    #plt.axis([0, 50, 0, 50])
pos_mst_visited = []
pos_avg_visited = []
pos_lst_visited = []

for pos in dictionary_positions:
    for s in pos.split():
        #x,z = s.split(",")
        #origin = (0.0,0.0)
        #x,z = rotate(int(x),int(z), origin )
        if(dictionary_positions[s] >= 1 and dictionary_positions[s] <= 3):
            pos_lst_visited.append(pos)
            #plt.plot(int(x), int(z), 'ys')
        if(dictionary_positions[s] >= 4 and dictionary_positions[s] <= 7):
            #plt.plot(int(x), int(z), 'bs')
            pos_avg_visited.append(pos)
        if(dictionary_positions[s] >= 8):
            #plt.plot(int(x), int(z), 'rs')
            pos_mst_visited.append(pos)

for i in range(len(pos_lst_visited)):
    for s in pos_lst_visited[i].split():
        x,z = s.split(",")
        origin = (0.0,0.0)
        x,z = rotate(int(x),int(z), origin)
        plt.plot(int(x), int(z), 'ys')

for i in range(len(pos_avg_visited)):
    for s in pos_avg_visited[i].split():
        x,z = s.split(",")
        origin = (0.0,0.0)
        x,z = rotate(int(x),int(z), origin)
        plt.plot(int(x), int(z), 'bs')

for i in range(len(pos_mst_visited)):
    for s in pos_mst_visited[i].split():
        x,z = s.split(",")
        origin = (0.0,0.0)
        x,z = rotate(int(x),int(z), origin)
        plt.plot(int(x), int(z), 'rs')

if os.path.isfile(inputDir2 + "/" + fileName2):
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
    
plt.show()






