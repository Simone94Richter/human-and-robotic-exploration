import json
import os
#import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import scipy.spatial
import math
from scipy.cluster.hierarchy import dendrogram, linkage

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

def rotate(x,y, origin=(0,0)):
    # shift to origin
    x1 = x #- origin[0]
    y1 = y #- origin[1]

    #rotate
    x2 = y1
    y2 = -x1

    #if os.path.isfile(inputDir + "/" + fileName + str(i) + "t.txt"):
    #    with open(inputDir + "/" + fileName + str(i) + "t.txt") as json_file:
    #       data = json.load(json_file)
    if(map_name == "uffici2.map"):
                # shift back
        x3 = x2
        y3 = y2 +53
    if(map_name == "open2.map"):
                # shift back
        x3 = x2
        y3 = y2 +57
    if(map_name == "open1.map"):
                # shift back
        x3 = x2
        y3 = y2 +48
    if(map_name == "uffici1.map"):
        x3 = x2
        y3 = y2 +54 

    return x3, y3

def levenshtein(s, t):
        #''' From Wikipedia article; Iterative with two matrix rows. '''
    if s == t: return 0
    elif len(s) == 0: return len(t)
    elif len(t) == 0: return len(s)
    v0 = [None] * (len(t) + 1)
    v1 = [None] * (len(t) + 1)
    for i in range(len(v0)):
        v0[i] = i
    for i in range(len(s)):
        v1[0] = i + 1
        for j in range(len(t)):
            cost = 0 if s[i] == t[j] else 1
            v1[j + 1] = min(v1[j] + 1, v0[j + 1] + 1, v0[j] + cost)
        for j in range(len(v0)):
            v0[j] = v1[j]
                
    return v1[len(t)]

def e_erp(t0,t1,g):
    """
    Usage
    -----
    The Edit distance with Real Penalty between trajectory t0 and t1.

    Parameters
    ----------
    param t0 : len(t0)x2 numpy_array
    param t1 : len(t1)x2 numpy_array

    Returns
    -------
    dtw : float
          The Dynamic-Time Warping distance between trajectory t0 and t1
    """

    n0 = len(t0)
    n1 = len(t1)
    C=np.zeros((n0+1,n1+1))

    t0_float = []
    t1_float = []

    k = 0
    while(k < n0):
        pos1 = t0[k]
        #pos2 = t1[k]
        x1, y1 = pos1.split(",")
        #x2, y2 = pos2.split(",")
        coord1 = [(float(x1), float(y1))]
        #coord2 = [(float(x2), float(y2))]
        t0_float.append(coord1)
        #t1_float.append(coord2)
        k = k + 1

    k = 0
    while(k < n1):
        #pos1 = t0[k]
        pos2 = t1[k]
        #x1, y1 = pos1.split(",")
        x2, y2 = pos2.split(",")
        #coord1 = [(float(x1), float(y1))]
        coord2 = [(float(x2), float(y2))]
        #t0_float.append(coord1)
        t1_float.append(coord2)
        k = k + 1
    
    #print(str(len(t0)) + ", " + str(len(t1)))

    C[1:,0]=sum(map(lambda x : abs(euc_dist(g,x)),t0_float))
    C[0,1:]=sum(map(lambda y : abs(euc_dist(g,y)),t1_float))
    for i in np.arange(n0)+1:
        for j in np.arange(n1)+1:
            derp0 = C[i-1,j] + euc_dist(t0_float[i-1],g)
            derp1 = C[i,j-1] + euc_dist(g,t1_float[j-1])
            derp01 = C[i-1,j-1] + euc_dist(t0_float[i-1],t1_float[j-1])
            C[i,j] = min(derp0,derp1,derp01)
    erp = C[n0,n1]
    return erp

def euc_dist(pt1,pt2):

    return scipy.spatial.distance.cdist(pt1, pt2, 'euclidean')[0][0]

def euclidean_dist_metric(path1_rescaled, path2_rescaled):
    
    euclidean_dist = []
    k = 0

    while (k < len_max):
        pos1 = path1_rescaled[k]
        pos2 = path2_rescaled[k]
        x1, y1 = pos1.split(",")
        x2, y2 = pos2.split(",")
            #coord1.append((float(x1), float(y1)))
            #coord2.append((float(x2), float(y2)))
            #coord1Array.append((float(x1), float(y1))) 
            #coord2Array.append((float(x2), float(y2)))
        coord1 = [(float(x1), float(y1))]
        coord2 = [(float(x2), float(y2))]
        euclidean_dist.append(euc_dist(coord1, coord2))
        k = k + 1

    total_dist = 0
    for num in euclidean_dist:
        total_dist = total_dist + num

    return total_dist

def _c(ca,i,j,p,q):

    x1, y1 = p[i].split(",")
    x2, y2 = q[j].split(",")
    if ca[i,j] > -1:
        return ca[i,j]
    elif i == 0 and j == 0:
        ca[i,j] = euc_dist([(int(x1),int(y1))], [(int(x2),int(y2))])
    elif i > 0 and j == 0:
        ca[i,j] = max( _c(ca,i-1,0,p,q), euc_dist([(int(x1),int(y1))], [(int(x2),int(y2))]))
    elif i == 0 and j > 0:
        ca[i,j] = max( _c(ca,0,j-1,p,q), euc_dist([(int(x1),int(y1))], [(int(x2),int(y2))]))
    elif i > 0 and j > 0:
        ca[i,j] = max(                                                     \
            min(                                                           \
                _c(ca,i-1,j,p,q),                                          \
                _c(ca,i-1,j-1,p,q),                                        \
                _c(ca,i,j-1,p,q)                                           \
            ),                                                             \
            euc_dist([(int(x1),int(y1))], [(int(x2),int(y2))])                                           \
            )                                                          
    else:
        ca[i,j] = float('inf')

    return ca[i,j]

def frechetDist(p,q):

    len_p = len(p)
    len_q = len(q)

    if len_p == 0 or len_q == 0:
        raise ValueError('Input curves are empty.')

    if len_p != len_q or len(p[0]) != len(q[0]):
        raise ValueError('Input curves do not have the same dimensions.')

    ca    = ( np.ones((len_p,len_q), dtype=np.float64) * -1 ) 
    dist = _c(ca,len_p-1,len_q-1,p,q)

    return dist

def lcs(a, b):
    lengths = [[0 for j in range(len(b)+1)] for i in range(len(a)+1)]
    # row 0 and column 0 are initialized to 0 already
    for i, x in enumerate(a):
        for j, y in enumerate(b):
            if x == y:
                lengths[i+1][j+1] = lengths[i][j] + 1
            else:
                lengths[i+1][j+1] = max(lengths[i+1][j], lengths[i][j+1])
    # read the substring out from the matrix
    result = ""
    x, y = len(a), len(b)
    while x != 0 and y != 0:
        if lengths[x][y] == lengths[x-1][y]:
            x -= 1
        elif lengths[x][y] == lengths[x][y-1]:
            y -= 1
        else:
            assert a[x-1] == b[y-1]
            result = a[x-1] + result
            x -= 1
            y -= 1

    return result

def distance_dtw(s1, s2, window=None, max_dist=None, max_step=None, max_length_diff=None, penalty=None, psi=None, use_c=False):

    """
    Dynamic Time Warping.

    This function keeps a compact matrix, not the full warping paths matrix.

    :param s1: First sequence
    :param s2: Second sequence
    :param window: Only allow for maximal shifts from the two diagonals smaller than this number.
    
    It includes the diagonal, meaning that an Euclidean distance is obtained by setting weight=1.

    :param max_dist: Stop if the returned values will be larger than this value
    :param max_step: Do not allow steps larger than this value
    :param max_length_diff: Return infinity if length of two series is larger
    :param penalty: Penalty to add if compression or expansion is applied
    :param psi: Psi relaxation parameter (ignore start and end of matching).

        Useful for cyclical series.

    :param use_c: Use fast pure c compiled functions

    Returns: DTW distance

    """

    r, c = len(s1), len(s2)
    k = 0

    s1_float = []
    s2_float = []

    while(k < r):
        pos1 = s1[k]
        #pos2 = t1[k]
        x1, y1 = pos1.split(",")
        #x2, y2 = pos2.split(",")
        coord1 = [(float(x1), float(y1))]
        #coord2 = [(float(x2), float(y2))]
        s1_float.append(coord1)
        #t1_float.append(coord2)
        k = k + 1

    k = 0
    while(k < c):
        pos1 = s2[k]
        #pos2 = t1[k]
        x1, y1 = pos1.split(",")
        #x2, y2 = pos2.split(",")
        coord1 = [(float(x1), float(y1))]
        #coord2 = [(float(x2), float(y2))]
        s2_float.append(coord1)
        #t1_float.append(coord2)
        k = k + 1

    if max_length_diff is not None and abs(r - c) > max_length_diff:
        return np.inf

    if window is None:
        window = max(r, c)

    if not max_step:
        max_step = np.inf
    else:
        max_step *= max_step

    if not max_dist:
        max_dist = np.inf
    else:
        max_dist *= max_dist

    if not penalty:
        penalty = 0
    else:
        penalty *= penalty

    if psi is None:
        psi = 0

    length = min(c + 1, abs(r - c) + 2 * (window - 1) + 1 + 1 + 1)
    # print("length (py) = {}".format(length))
    dtw = np.full((2, length), np.inf)
    # dtw[0, 0] = 0

    for i in range(psi + 1):
        dtw[0, i] = 0

    last_under_max_dist = 0
    skip = 0
    i0 = 1
    i1 = 0
    psi_shortest = np.inf

    for i in range(r):
        # print("i={}".format(i))
        # print(dtw)
        if last_under_max_dist == -1:
            prev_last_under_max_dist = np.inf
        else:
            prev_last_under_max_dist = last_under_max_dist

        last_under_max_dist = -1
        skipp = skip
        skip = max(0, i - max(0, r - c) - window + 1)
        i0 = 1 - i0
        i1 = 1 - i1
        dtw[i1, :] = np.inf
        j_start = max(0, i - max(0, r - c) - window + 1)
        j_end = min(c, i + max(0, c - r) + window)

        if dtw.shape[1] == c + 1:
            skip = 0
        if psi != 0 and j_start == 0 and i < psi:
            dtw[i1, 0] = 0

        for j in range(j_start, j_end):
            d = euc_dist(s1_float[i], s2_float[j])**2
            if d > max_step:
                continue

            assert j + 1 - skip >= 0
            assert j - skipp >= 0
            assert j + 1 - skipp >= 0
            assert j - skip >= 0

            dtw[i1, j + 1 - skip] = d + min(dtw[i0, j - skipp],
                                            dtw[i0, j + 1 - skipp] + penalty,
                                            dtw[i1, j - skip] + penalty)

            # print('({},{}), ({},{}), ({},{})'.format(i0, j - skipp, i0, j + 1 - skipp, i1, j - skip))

            # print('{}, {}, {}'.format(dtw[i0, j - skipp], dtw[i0, j + 1 - skipp], dtw[i1, j - skip]))

            # print('i={}, j={}, d={}, skip={}, skipp={}'.format(i,j,d,skip,skipp))

            # print(dtw)

            if dtw[i1, j + 1 - skip] <= max_dist:
                last_under_max_dist = j
            else:
                # print('above max_dist', dtw[i1, j + 1 - skip], i1, j + 1 - skip)
                dtw[i1, j + 1 - skip] = np.inf

                if prev_last_under_max_dist + 1 - skipp < j + 1 - skip:
                    # print("break")
                    break

        if last_under_max_dist == -1:
            # print('early stop')
            # print(dtw)
            return np.inf

        if psi != 0 and j_end == len(s2) and len(s1) - 1 - i <= psi:
            psi_shortest = min(psi_shortest, dtw[i1, length - 1])

    if psi == 0:
        d = math.sqrt(dtw[i1, min(c, c + window - 1) - skip])
    else:
        ic = min(c, c + window - 1) - skip
        vc = dtw[i1, ic - psi:ic + 1]
        d = min(np.min(vc), psi_shortest)
        d = math.sqrt(d)

    return d


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

### Calculating distance

i = 0
c = 1
i_index = 0
finished = False
len_array = len(dictionary_path)
dist_array = [[0 for x in range(len_array)] for y in range(len_array)]

while(i < len_array):
    path1 = dictionary_path[i]
    j =  i + 1
    while(j < len_array):
        path2 = dictionary_path[j]
        
        len1 = len(path1) 
        len2 = len(path2)

        if len1 > len2:
            len_ = len2
            len_max = len1
            path_min = path2 
            path_max = path1
            re_scale_factor = float(len2)/float(len1) 
        else:
            len_ = len1
            len_max = len2
            path_min = path1 
            path_max = path2
            re_scale_factor = float(len1)/float(len2)

        print(str(re_scale_factor))
        #re-scaling both path
        v = 0.0 
        path1_rescaled = []
        path2_rescaled = []
        
        #print(str(v) + ", " + str(w))
        while(v < float(len_)):
            path1_rescaled.append(path_min[int(v)])
            v = float(v) + re_scale_factor

        v = 0.0
        while(v < float(len_max)):
            path2_rescaled.append(path_max[int(v)])
            v = v + 1

        #because sometimes one of the path (rescaled) is longer by one than the other, we add to the shortest a copy of the last element
        if(len(path1_rescaled) > len(path2_rescaled)):
            path2_rescaled.append(path2_rescaled[len(path2_rescaled)-1])

        if(len(path2_rescaled) > len(path1_rescaled)):
            path1_rescaled.append(path1_rescaled[len(path1_rescaled)-1])

        distanceL = levenshtein(path1, path2)
        distanceERP = round(e_erp(path1, path2, np.zeros((1, 2), dtype=float)), 5) 
        distanceF = round(frechetDist(path1_rescaled, path2_rescaled), 5)
        distanceLCS = len(lcs(path1, path2))
        distanceE = round(euclidean_dist_metric(path1_rescaled, path2_rescaled), 5)
        distanceDTW = round(distance_dtw(path1, path2), 5)


        advise = "Simple distance between the path " + str(i) + " and " + str(j) + " :"
        advise_time = "Time of the paths taken in consideration: " + str(dictionary_time_path[i]) + " " + str(dictionary_time_path[j])
        print(advise)
        #print(coord1Array)
        #print(coord2Array)

        total_dist = 0
        #for num in dist:
        #    total_dist = total_dist + num

        print(distanceL)
        print(distanceERP)
        print(distanceF)
        print(distanceLCS)
        print(distanceE)
        print(distanceDTW)
        #print(len(path1))
        #print(len(path2))
        print(advise_time)

        #dist_array[i][j] = distance
        #dist_array.append(total_dist)

        #inserire qui il plot dei due path
        num_plot = len_array * len_array
        num_plot = (num_plot - len_array) / 2
        num_rows = (float(num_plot / 2))
        if(num_rows > int(num_rows)):
            num_rows = int(num_rows) + 1
        #print(str(int(num_rows)))

        while(i_index < 6):
            plt.subplot(num_rows, 6, c)
            with open(inputDir2 + "/" + fileName2) as f:
                array = []
                content = f.readlines()

                content = [x.strip() for x in content]
        
                n = 0
                for line in content:
            
                    array = line.split(',')
            
                    for m in range(len(array)):
                        a = int(array[m])
                        if(a == 1):
                            plt.plot(m, len(content)-n, 'ws')
                
                        if(a == 4):
                            plt.plot(m, len(content)-n , 'gs')

                    n = n + 1    

            for k in range(len(path1)):
                for s in path1[k].split():
                    x,z = s.split(",")
                    origin = (0.0,0.0)
                    x, z = rotate(int(x),int(z), origin )
                    #x = maxlen-int(x)
                    #z = int(z)
                    #print(x, z)
                    if k+1 < len(path1):
                        a,b = path1[k+1].split(",")
                        a, b = rotate(int(a),int(b), origin )
                    #a = int(a)
                    #b = int(b)
                    #if(frag == 1):
                        plt.plot([x, a], [z, b], 'k-')
                    
        
            for k in range(len(path2)):
                for s in path2[k].split():
                    x,z = s.split(",")
                    origin = (0.0,0.0)
                    x, z = rotate(int(x),int(z), origin )
                    #x = maxlen-int(x)
                    #z = int(z)
                    #print(x, z)
                    if k+1 < len(path2):
                        a,b = path2[k+1].split(",")
                        a, b = rotate(int(a),int(b), origin )
                    #a = int(a)
                    #b = int(b)
                    #if(frag == 1):
                        plt.plot([x, a], [z, b], 'r-')

            if(i_index == 0):
                plt.title('Distance (L): ' + str(distanceL), fontsize = 8)
            elif(i_index == 1):
                plt.title('Distance (F): ' + str(distanceF), fontsize = 8)
            elif(i_index == 2):
                plt.title('Distance (LCS): ' + str(distanceLCS), fontsize = 8)
            elif(i_index == 3):
                plt.title('Distance (E): ' + str(distanceE), fontsize = 8)
            elif(i_index == 4):
                plt.title('Distance(ERP): ' + str(distanceERP), fontsize = 8)
            elif(i_index == 5):
                plt.title('Distance(DTW): ' + str(distanceDTW), fontsize = 8)
        
            i_index = i_index + 1

            #if(j + 1 >= len_array and i + 2 >= len_array):
            #    finished = True

            if(c < num_rows * 6):
                c = c + 1
            elif(c >= num_rows * 6):
                plt.subplots_adjust(0.12, 0.06, 0.82, 0.92, 0.59, 0.99)
                plt.show()
                c = 1

        j = j + 1

        if(i_index >= 6):
            i_index = 0
    
    i = i + 1 

#for x in range(len_array):
#    print(dist_array[x])

plt.subplots_adjust(0.12, 0.06, 0.82, 0.92, 0.59, 0.99)
plt.show()

##### clustering part #####

#Z = linkage(dist_array, 'ward')
#plt.title('Hierarchical Clustering Dendrogram')
#plt.xlabel('sample index')
#plt.ylabel('distance')
#dendrogram(Z)
#plt.show()