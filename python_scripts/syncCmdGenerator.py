import random

# 0无输入
# 1移动
# 2格挡，使用后20帧无输入
# 3轻攻击，使用和15帧无输入
# 4重攻击，使用后25帧无输入
input_list = [0, 1, 2, 3, 4]
weight_normal = [30, 50 ,10, 10, 0]
weight_idle = [100, 0, 0, 0, 0]
weight_defend = [10, 20, 20, 10 ,40]
count_case = [0, 0, 15, 10, 20]

player1_status = 0
player2_status = 0
player1_count = 0
player2_count = 0

string1 = ""
string2 = ""

length = 5400
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
        player1_status = random.choices(input_list, player1_weight)[0]
        player1_count = count_case[player1_status]

    if player2_count == 0:
        player2_status = random.choices(input_list, player2_weight)[0]
        player2_count = count_case[player2_status]

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

with open("sample_0.txt", "w") as file:
    file.write(string1)

with open("sample_1.txt", "w") as file:
    file.write(string2)
#'''