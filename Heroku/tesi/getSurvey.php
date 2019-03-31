<?php

$db_conn = pg_connect("dbname=da46hhlnl2gsi5 host=ec2-54-225-249-161.compute-1.amazonaws.com port
    =5432 user=webojtnmkhwhha password=b66f35bec3a21fad615f55825c819ff2de4a82a0733aa890ce8d520e64c6f336 sslmode=require");

//$_POST = json_decode(file_get_contents('php://input'), true);
//$id = $_POST["id"];
$response = array();

$results = pg_query($db_conn, "SELECT * FROM surveyexploration");

if($results){
    if(pg_num_rows($results) > 0){
        $response["survey"] = array();
    
        while($row = pg_fetch_assoc($results)){
    
            array_push($response["survey"], $row);
            //$i = $i + 1
        }
        echo json_encode($response["survey"]);
        
    }
}else{
    echo("Wrong query");
}
?>