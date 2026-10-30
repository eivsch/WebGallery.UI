How to install face_recognition:
- Downloaded pre-compiled "dlib wheel" from github for python 3.14
- As per instructions here: https://github.com/ageitgey/face_recognition/issues/608
    - pip install setuptools<81
    - pip install .\dlib-20.0.99-cp314-cp314-win_amd64.whl
    - pip install face_recognition
    - pip install "fastapi[standard]"