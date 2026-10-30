import os
import face_recognition

def load_known_faces(known_people_folder):
    known_faces = {}
    for known_face in os.listdir(known_people_folder):
        known_image = face_recognition.load_image_file(os.path.join(known_people_folder, known_face))
        known_face_encoding = face_recognition.face_encodings(known_image)[0]
        known_faces[known_face] = known_face_encoding
    return known_faces

print('Running face recognition...')

unknown_people_folder = "C:/WebGallery/Data/gallery/dogf"
known_people_folder = "C:/WebGallery/Data/KnownPeople"

print(f"Loading known faces from: {known_people_folder}")
known_faces = load_known_faces(known_people_folder)

for image_path in os.listdir(unknown_people_folder):
    #print(f"Processing image: {image_path}")
    image = face_recognition.load_image_file(os.path.join(unknown_people_folder, image_path))
    unknown_faces = face_recognition.face_locations(image) 

    if len(unknown_faces) == 0:
        continue

    print(f"Found {len(unknown_faces)} face(s) in {image_path}")

    # Loop through the faces in the image and compare them to the known faces
    for known_face, known_face_encoding in known_faces.items():
        for unknown_face in unknown_faces:
            unknown_face_encoding = face_recognition.face_encodings(image, [unknown_face])[0]
            #results = face_recognition.compare_faces([known_face_encoding], unknown_face_encoding)
            face_distance = face_recognition.face_distance([known_face_encoding], unknown_face_encoding)[0]

            if face_distance < 0.5: #and face_distance <= 0.52:
                print(f"Match found: {known_face} in {image_path} with distance level {face_distance}")
            