# Train multiple images per person
# Find and recognize faces in an image using a SVC with scikit-learn

"""
Structure:
        <test_image>.jpg
        <train_dir>/
            <person_1>/
                <person_1_face-1>.jpg
                <person_1_face-2>.jpg
                .
                .
                <person_1_face-n>.jpg
           <person_2>/
                <person_2_face-1>.jpg
                <person_2_face-2>.jpg
                .
                .
                <person_2_face-n>.jpg
            .
            .
            <person_n>/
                <person_n_face-1>.jpg
                <person_n_face-2>.jpg
                .
                .
                <person_n_face-n>.jpg
"""

import face_recognition
from sklearn import svm
import os
import pickle

def train_classifier(train_dir_path):
    # The training data would be all the face encodings from all the known images and the labels are their names
    encodings = []
    names = []
    train_dir = os.listdir(train_dir_path)

    # Loop through each person in the training directory
    for person in train_dir:
        print(f"Processing training images for: {person}")
        pix = os.listdir(train_dir_path + "/" + person)

        # Loop through each training image for the current person
        for person_img in pix:
            # Get the face encodings for the face in each image file
            face = face_recognition.load_image_file(train_dir_path + "/" + person + "/" + person_img)
            face_bounding_boxes = face_recognition.face_locations(face)

            # If training image contains exactly one face
            if len(face_bounding_boxes) == 1:
                face_enc = face_recognition.face_encodings(face)[0]
                # Add face encoding for current image with corresponding label (name) to the training data
                encodings.append(face_enc)
                names.append(person)
            else:
                print(person + "/" + person_img + " was skipped and can't be used for training")

    clf = svm.SVC(gamma='scale')
    clf.fit(encodings, names)
    return clf, encodings


def load_or_train_classifier(train_dir_path, model_path):
    if os.path.exists(model_path):
        with open(model_path, "rb") as model_file:
            cached = pickle.load(model_file)
        print(f"Loaded cached classifier from: {model_path}")
        return cached["clf"], cached["encodings"]

    clf, encodings = train_classifier(train_dir_path)
    with open(model_path, "wb") as model_file:
        pickle.dump({"clf": clf, "encodings": encodings}, model_file)
    print(f"Trained and saved classifier to: {model_path}")
    return clf, encodings

def predict_faces_in_image(clf, known_encodings, test_image_path):
    # Load the test image with unknown faces into a numpy array
    test_image = face_recognition.load_image_file(test_image_path)
    unknown_faces = face_recognition.face_locations(test_image)
    
    no = len(unknown_faces)
    if no == 0:
        print(f"No faces found in {test_image_path}")
        return
    
    predictions = []
    # Predict all the faces in the test image using the trained classifier
    for i in range(no):
        test_image_enc = face_recognition.face_encodings(test_image)[i]
        predicted_name = clf.predict([test_image_enc])[0]

        # Report how close this face is to the nearest training sample.
        distances = face_recognition.face_distance(known_encodings, test_image_enc)
        best_distance = float(min(distances)) if len(distances) > 0 else None

        if best_distance is None:
            print(f"{predicted_name} (distance: N/A)")
        else:
            predictions.append((predicted_name, best_distance))
    
    return predictions


# Training directory
train_dir_path = 'C:/WebGallery/Data/KnownPeople/train_dir'
model_path = os.path.join(os.path.dirname(__file__), "face_svc_model.pkl")

clf, known_encodings = load_or_train_classifier(train_dir_path, model_path)

count = 0
# Load the test image with unknown faces into a numpy array
test_dir_path = 'C:/WebGallery/Data/TestFolderPeople'
for test_image_name in os.listdir(test_dir_path):
    print(f"Processing test image: {test_image_name}")
    predictions = predict_faces_in_image(clf, known_encodings, os.path.join(test_dir_path, test_image_name))
    if predictions is None:
        continue

    for predicted_name, best_distance in predictions:
        print(f"- Found {predicted_name} (distance {best_distance})")
        if best_distance < 0.51:
            count += 1

print(f"Total recognized faces with distance < 0.51: {count} of total {len(os.listdir(test_dir_path))} test images")