import json
import os
#import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import scipy.spatial

inputDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/DownloadedResults"
inputDir2 = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/Python"
fileName = "Result"
fileName2 = ""
index = 1
i = 0
mappa = input("Inserire il numero della mappa sulla quale lavorare (1 per uffici2, 2 per uffici1, 3 per open1 e 4 per open2): ")
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

dictionary_path = {}
dictionary_time_path = {}

height_map = 0
width_map = 0
maximum_dist = 0


if os.path.isfile(inputDir2 + "/" + fileName2):
    with open(inputDir2 + "/" + fileName2) as map_file:
        array = []
        content = map_file.readlines()
        content = [x.strip() for x in content]
        height_map = len(content)
        array = content[0].split(',')
        width_map = len(array)
        print(str(height_map))
        print(str(width_map))
        maximum_dist = np.sqrt(height_map * height_map + width_map * width_map)
        print(str(maximum_dist))


while(finish == False):
    if os.path.isfile(inputDir + "/" + fileName + str(index) + "t.txt"):
        with open(inputDir + "/" + fileName + str(index) + "t.txt") as json_file:
                data = json.load(json_file)
                if(data['mapName'][0] == map_name):
                    array_pos = data['position']
                    dictionary_path[i] = array_pos
                    dictionary_time_path[i] = data['time']
                    print("Numero" + str(i) + ": Result"+ str(index))
                    i = i + 1
                
                index = index + 1
    
    else:
        finish = True

i = 0
len_array = len(dictionary_path)

while(i < len_array):
    path1 = dictionary_path[i]
    j = i + 1
    while(j < len_array):
        path2 = dictionary_path[j]
        len1 = len(path1) 
        len2 = len(path2)

        if len1 > len2:
            len_ = len2
        else:
            len_ = len1
        
        k = 0
        coord1Array = []
        coord2Array = []
        dist = []

        while (k < len_):
            pos1 = path1[k]
            pos2 = path2[k]
            x1, y1 = pos1.split(",")
            x2, y2 = pos2.split(",")
            #coord1.append((float(x1), float(y1)))
            #coord2.append((float(x2), float(y2)))
            coord1Array.append((float(x1), float(y1))) 
            coord2Array.append((float(x2), float(y2)))
            coord1 = [(float(x1), float(y1))]
            coord2 = [(float(x2), float(y2))]
            dist.append(scipy.spatial.distance.cdist(coord1, coord2, 'euclidean')[0][0])
            k = k + 1

        if len_ == len1:
            while(k < len2):
                pos = path2[k]
                x, y = pos.split(",")
                coord2Array.append((float(x), float(y)))
                dist.append(maximum_dist)
                k = k + 1
        else:
            while(k < len1):
                pos = path1[k]
                x, y = pos.split(",")
                coord1Array.append((float(x), float(y)))
                dist.append(maximum_dist)
                k = k + 1

        advise = "Simple distance between the path " + str(i) + " and " + str(j) + " :"
        advise_time = "Time of the paths taken in consideration: " + str(dictionary_time_path[i]) + " " + str(dictionary_time_path[j])
        print(advise)
        #print(coord1Array)
        #print(coord2Array)

        total_dist = 0
        for num in dist:
            total_dist = total_dist + num

        print(total_dist)
        print(advise_time)

        j = j + 1
    
    i = i + 1 


#path1 = dictionary_path[0]
#path2 = dictionary_path[1]
#len1 = len(path1) 
#len2 = len(path2)

#if len1 > len2:
#    len = len2
#else:
#    len = len1

#k = 0
#coord1Array = []
#coord2Array = []
#dist = []

#while (k < len):
#    pos1 = path1[k]
#    pos2 = path2[k]
#    x1, y1 = pos1.split(",")
#    x2, y2 = pos2.split(",")
    #coord1.append((float(x1), float(y1)))
    #coord2.append((float(x2), float(y2)))
#    coord1Array.append((float(x1), float(y1))) 
#    coord2Array.append((float(x2), float(y2)))
#    coord1 = [(float(x1), float(y1))]
#    coord2 = [(float(x2), float(y2))]
#    dist.append(scipy.spatial.distance.cdist(coord1, coord2, 'euclidean')[0][0])
#    k = k + 1

#if len == len1:
#    while(k < len2):
#        pos = path2[k]
#        x, y = pos.split(",")
#        coord2Array.append((float(x), float(y)))
#        dist.append(50)
#        k = k + 1
#else:
#    while(k < len1):
#        pos = path1[k]
#        x, y = pos.split(",")
#        coord1Array.append((float(x), float(y)))
#        dist.append(50)
#        k = k + 1


#print(coord1Array)
#print(coord2Array)
#print(dist)