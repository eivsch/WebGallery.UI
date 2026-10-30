$target = "C:\WebGallery\App\FaceRecognitionApi"
New-Item -ItemType Directory -Path $target -Force | Out-Null

$source = "C:\Git\MinimalGallery\MinimalGallery.API\FaceRecognition"
Copy-Item "$source\main.py" $target -Force
Copy-Item "$source\face_recognition_knn.py" $target -Force
Copy-Item "$source\trained_knn_model.clf" $target -Force
Copy-Item "$source\dlib-20.0.99-cp314-cp314-win_amd64.whl" $target -Force
Copy-Item "$source\requirements.txt" $target -Force
Copy-Item "$source\run.ps1" $target -Force