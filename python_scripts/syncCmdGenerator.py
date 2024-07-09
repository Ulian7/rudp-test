import random
import argparse
# 0无输入
# 1移动
# 2格挡，使用后20帧无输入
# 3轻攻击，使用和15帧无输入
# 4重攻击，使用后25帧无输入
input_list1 = [0, 1]
input_list2 = [0, 2, 3, 4]

weight_normal = [40, 20, 20, 20]
weight_left = [30, 70]
weight_defend = [25, 40, 15, 20]
weight_idle = [100, 0, 0, 0]
count_case = [0, 0, 15, 10, 20]

parser = argparse.ArgumentParser();
parser.add_argument('-vfps', type=int);
parser.add_argument("-seconds", type=int);
args = parser.parse_args();

if (args.vfps >= 30):
    count_case *=  int(args.vfps / 30)


player1_status = 0
player2_status = 0
player1_count = 0
player2_count = 0

string1 = ""
string2 = ""

length = args.vfps * args.seconds;

#'''
for i in range(length):
    if player1_status > 1 and player1_count == 0:
        player1_status = 0
    if player2_status > 1 and player2_count == 0:
        player1_status = 0

    if player1_status > 1:
        player1_weight = weight_idle
    elif player2_status > 2:
        player1_weight = weight_defend
    else:
        player1_weight = weight_normal

    if player2_status > 1:
        player2_weight = weight_idle
    elif player1_status > 2:
        player2_weight = weight_defend
    else:
        player2_weight = weight_normal

    if player1_count == 0:
        player1_status = random.choices(input_list2, player1_weight)[0]
        player1_count = count_case[player1_status]

    if player2_count == 0:
        player2_status = random.choices(input_list2, player2_weight)[0]
        player2_count = count_case[player2_status]

    string1 += str(random.choices(input_list1, weight_left)[0])
    string2 += str(random.choices(input_list1, weight_left)[0])

    if (player1_count == count_case[player1_status]):
        string1 += str(player1_status)
    else:
        string1 += "0"

    if (player2_count == count_case[player2_status]):
        string2 += str(player2_status)
    else:
        string2 += "0"

    if player1_count > 0:
        player1_count -= 1
    if player2_count > 0:
        player2_count -= 1

with open("sample_0_vfps_" + str(args.vfps) + "_seconds_" + str(args.seconds) + ".txt", "w") as file:
    file.write(string1)

with open("sample_1_vfps_" + str(args.vfps) + "_seconds_" + str(args.seconds) + ".txt", "w") as file:
    file.write(string2)
#'''