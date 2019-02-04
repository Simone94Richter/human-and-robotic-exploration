import json
import os
#import pandas as pd
import numpy as np
import math
import matplotlib.pyplot as plt

robotDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/Unity/Project Arena/Assets/Results/ExperimentSamples"
humanDir = "C:/Users/princ/OneDrive/Documenti/human-and-robotic-exploration/human-and-robotic-exploration/DownloadedResults"
fileName = "resultMapNum"
fileName2 = "resultPositionNum"
humanfileName = "Result"
index = 1
keepGoing = True
indexBestMatch = 1
distanceBestMatch = math.inf

#Porre scelta per il file umano da confrontare

def euc_dist(pt1,pt2):

    return scipy.spatial.distance.cdist(pt1, pt2, 'euclidean')[0][0]

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

if os.path.isfile(inputDir + "/" + humanfileName + "t.txt"):
    with open(inputDir + "/" + humanfileName + "t.txt") as json_file:
        data = json.load(json_file)
        map_name = data['mapName']
        human_array_pos = data['position']

while(keepGoing == True):
    if os.path.isfile(robotDir + "/" + fileName + str(index) + ".json") and os.path.isfile(robotDir + "/" + fileName2 + str(index) + ".json"):
        with open(robotDir + "/" + fileName2 + str(index) + ".json") as json_file:
            data = json.load(json_file)
            if(data['mapName'] == map_name and data['time'] > 0):
                robot_array_pos = data['position']
                alpha = data['alpha']
                beta = data['beta']
                delta = data['delta']
                distanceDTW = round(distance_dtw(human_array_pos, robot_array_pos), 5)
                if(distanceDTW < distanceBestMatch):
                    indexBestMatch = index

        index = index + 1
    
    else:
        keepGoing = False

# rappresentare ledue traiettorie sul plot






