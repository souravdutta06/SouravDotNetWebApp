pipeline {
    agent any
    options {
        skipDefaultCheckout true
    }
    environment {
        DOCKER_IMAGE = "souravdutta06/sourav-dotnet-webapp"
        DOCKER_TAG = "latest"
        APP_VM = "sysadmin@20.57.129.245"  // Replace with actual IP
    }
    stages {
        stage('Clean Workspace') {
            steps {
                cleanWs()
            }
        }
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        stage('Build') {
            steps {
                // Use sh for Linux/macOS, bat for Windows
                script {
                    if (isUnix()) {
                        sh 'dotnet build SouravDotNetWebApp.sln'
                    } else {
                        bat 'dotnet build SouravDotNetWebApp.sln'
                    }
                }
            }
        }
    
        stage('Build Docker Image') {
            steps {
                script {
                    dockerImage = docker.build("${DOCKER_IMAGE}:${DOCKER_TAG}")
                }
            }
        }
        stage('Push to Docker Hub') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'docker-hub-creds', usernameVariable: 'DOCKER_USER', passwordVariable: 'DOCKER_PWD')]) {
                    sh "echo ${DOCKER_PWD} | docker login -u ${DOCKER_USER} --password-stdin"
                     script { // Wrap method calls in a script block
                    dockerImage.push()
                     }
                }
            }
        }
       steps {
                sshagent(['app-vm-ssh-key']) {
                    sh """
                        ssh -o StrictHostKeyChecking=no ${APP_VM} "
                            docker pull ${DOCKER_IMAGE}:${DOCKER_TAG}
                            docker stop dotnet-app || echo 'No container to stop'
                            docker rm dotnet-app || echo 'No container to remove'
                            docker run -d \
                                --name dotnet-app \
                                --restart=always \
                                -p 80:80 \
                                --health-cmd='curl -f http://localhost:80 || exit 1' \
                                --health-interval=30s \
                                ${DOCKER_IMAGE}:${DOCKER_TAG}
                        "
                    """
                }
            }
    }
  
}
