<?php

$db_conn = pg_connect("dbname=da46hhlnl2gsi5 host=ec2-54-225-249-161.compute-1.amazonaws.com port
    =5432 user=webojtnmkhwhha password=b66f35bec3a21fad615f55825c819ff2de4a82a0733aa890ce8d520e64c6f336 sslmode=require");

$_POST = json_decode(file_get_contents('php://input'), true);
$position = array();
$rotation = array();
$time = array();
$position = $_POST["position"];
$rotation = $_POST["rotationY"];
$time = $_POST["time"];
$map_name = $_POST["mapName"];
if(empty($_POST["ip"]))
{
    $ip = $_SERVER['REMOTE_ADDR'];
} else $ip = $_POST["ip"];
$os = $_POST["os"];
$browser = get_browser();

$today = getdate();
$d = $today['mday'];
$m = $today['mon'];
$y = $today['year'];
$h = $today['hours'];
$min = $today['minutes'];
$s = $today['seconds'];
$created_at = "$d-$m-$y, $h:$min:$s";
$updated_at = $created_at;

$numb_rows = pg_query($db_conn, "SELECT * FROM robotTrajectory");

if(pg_num_rows($numb_rows) == 0){

    $string_pos = "(".$position[0].")";
    $string_rot = "(".$rotation[0].")";
    $string_time = "(".$time.")";
    for($i = 1; $i < count($position); $i++){
        $string_pos = $string_pos.", (".$position[$i].")";
    }
    for($i = 1; $i < count($rotation); $i++){
        $string_rot = $string_rot.", (".$rotation[$i].")";
    }
    $string_map = "(".$map_name.")";

    $query = pg_prepare($db_conn, "insert_query", "INSERT INTO robotTrajectory VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)");
    $result = pg_execute($db_conn, "insert_query", array(1, $string_pos, $string_rot, $string_time, $created_at, $updated_at, $string_map, $ip, $os, $browser));
    //"INSERT INTO commentsreviews VALUES (1, '$product', '$author', '$content', '$created_at', '$updated_at')"
}
else {
    
    //$highest_id_query = pg_query($db_conn, "SELECT * FROM robotmap");

    $new_id = (int)pg_num_rows($numb_rows) + 1;

    $string_pos = "(".$position[0].")";
    $string_rot = "(".$rotation[0].")";
    $string_time = "(".$time.")";
    for($i = 1; $i < count($position); $i++){
        $string_pos = $string_pos.", (".$position[$i].")";
    }
    for($i = 1; $i < count($rotation); $i++){
        $string_rot = $string_rot.", (".$rotation[$i].")";
    }
    $string_map = "(".$map_name.")";

    $query = pg_prepare($db_conn, "insert_query", "INSERT INTO robotTrajectory VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10)");
    $result = pg_execute($db_conn, "insert_query", array($new_id, $string_pos, $string_rot, $string_time, $created_at, $updated_at, $string_map, $ip, $os, $browser));
    //"INSERT INTO commentsreviews VALUES ('$new_id', '$product', '$author', '$content', '$created_at', '$updated_at')"
}

if($result){
    echo($ip);
} else if(!isset($position)){
    echo ("something wrong in uknown");
} else if(!isset($rotation)){
    echo("something wrong in goal");
} else if(!isset($time)){
    echo("something wrong in wall");
} else echo("something wrong");
?>