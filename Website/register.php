<?php
header('Access-Control-Allow-Origin: *');
header('Access-Control-Allow-Methods: POST');
header('Access-Control-Allow-Headers: Content-Type');
header('Content-Type: application/json');

// Cosmos DB Configuration - UPDATED WITH REAL CREDENTIALS
$cosmosEndpoint = "https://fallen-collective.documents.azure.com:443/";
$cosmosKey = "EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q==";
$databaseId = "DJBookingDB";
$containerId = "users";

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $input = json_decode(file_get_contents('php://input'), true);
    
    // Validate input
    if (!isset($input['username']) || !isset($input['password'])) {
        http_response_code(400);
        echo json_encode(['error' => 'Username and password required']);
        exit;
    }
    
    $username = trim($input['username']);
    $password = $input['password'];
    $fullName = $input['fullName'] ?? $username;
    $isDJ = $input['isDJ'] ?? false;
    $isVenueOwner = $input['isVenueOwner'] ?? false;
    
    // Validate username
    if (strlen($username) < 3 || strlen($username) > 20) {
        http_response_code(400);
        echo json_encode(['error' => 'Username must be 3-20 characters']);
        exit;
    }
    
    // Validate password
    if (strlen($password) < 6) {
        http_response_code(400);
        echo json_encode(['error' => 'Password must be at least 6 characters']);
        exit;
    }
    
    // Hash password (same as C# implementation)
    $passwordHash = hash('sha256', $password);
    
    // Create user document
    $userId = generateGuid();
    $userDoc = [
        'id' => $userId,
        'type' => 'User',
        'username' => $username,
        'passwordHash' => $passwordHash,
        'fullName' => $fullName,
        'role' => 'Guest',
        'isDJ' => $isDJ,
        'isVenueOwner' => $isVenueOwner,
        'isActive' => true,
        'isBanned' => false,
        'banStrikeCount' => 0,
        'isPermanentBan' => false,
        'isGloballyMuted' => false,
        'createdAt' => date('c'),
        'lastLogin' => null,
        'currentIP' => $_SERVER['REMOTE_ADDR'],
        'ipHistory' => [$_SERVER['REMOTE_ADDR']]
    ];
    
    // Insert into Cosmos DB
    $result = createCosmosDocument($cosmosEndpoint, $cosmosKey, $databaseId, $containerId, $userDoc, $username);
    
    if ($result['success']) {
        http_response_code(201);
        echo json_encode([
            'success' => true,
            'message' => 'Account created successfully!',
            'username' => $username
        ]);
    } else {
        http_response_code(500);
        echo json_encode([
            'error' => $result['error']
        ]);
    }
} else {
    http_response_code(405);
    echo json_encode(['error' => 'Method not allowed']);
}

function generateGuid() {
    return sprintf('%04x%04x-%04x-%04x-%04x-%04x%04x%04x',
        mt_rand(0, 0xffff), mt_rand(0, 0xffff),
        mt_rand(0, 0xffff),
        mt_rand(0, 0x0fff) | 0x4000,
        mt_rand(0, 0x3fff) | 0x8000,
        mt_rand(0, 0xffff), mt_rand(0, 0xffff), mt_rand(0, 0xffff)
    );
}

function createCosmosDocument($endpoint, $key, $databaseId, $containerId, $document, $partitionKey) {
    $resourceType = "docs";
    $resourceLink = "dbs/$databaseId/colls/$containerId";
    $date = gmdate('D, d M Y H:i:s T');
    
    // Generate authorization token
    $verb = "POST";
    $stringToSign = "$verb\n$resourceType\n$resourceLink\n" . strtolower($date) . "\n\n";
    $signature = base64_encode(hash_hmac('sha256', $stringToSign, base64_decode($key), true));
    $authToken = urlencode("type=master&ver=1.0&sig=$signature");
    
    $url = trim($endpoint, '/') . "/$resourceLink/docs";
    
    $headers = [
        "Authorization: $authToken",
        "x-ms-date: $date",
        "x-ms-version: 2018-12-31",
        "x-ms-documentdb-partitionkey: [\"$partitionKey\"]",
        "Content-Type: application/json"
    ];
    
    $ch = curl_init($url);
    curl_setopt($ch, CURLOPT_CUSTOMREQUEST, "POST");
    curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($document));
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
    curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
    
    $response = curl_exec($ch);
    $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);
    
    if ($httpCode >= 200 && $httpCode < 300) {
        return ['success' => true];
    } else {
        return ['success' => false, 'error' => "Database error: HTTP $httpCode - $response"];
    }
}
?>