import React, { useEffect, useState } from 'react'
import { Box, Center, Flex, Grid, GridItem, HStack, Spacer, Image, Img, AspectRatio, Stack, Text, Button, Tag, TagLabel, Stat, StatLabel, StatNumber, StatHelpText, StatGroup, Heading, FormControl, FormLabel, Input, extendTheme, SimpleGrid, Checkbox, Select, RangeSlider, RangeSliderTrack, RangeSliderFilledTrack, RangeSliderThumb, SliderMark } from "@chakra-ui/react"
import { EventSearchResult } from '../contracts/backendApi'

interface IndexPageProps {
    events: EventSearchResult[]
}

const IndexPage = () => {

    const [sliderValue, setSliderValue] = useState(120)

    return (
        <>
            <Flex as="header" 
                position="fixed" 
                w="100%" 
                h="100px" 
                justifyContent={'center'}
                backgroundColor="green.600" 
                style={{transition: '0.2s', zIndex: 1000}}
            >
                <Flex direction={'row'} w={1200} h="100%" alignItems={'center'}>
                    <Box>

                    </Box>
                    <Spacer />
                    <Box>
                        <Text color='white' textAlign={'center'} fontSize='5xl' fontWeight={900}>Summer Olympic Games 2024</Text>
                    </Box>
                    <Spacer />
                    <Box>

                    </Box>
                </Flex>
            </Flex>
            <Grid 
                minH="100vh"
                bg="white" 
                templateRows="500px auto"
                templateColumns="auto 1200px auto"
            >
                <GridItem  colSpan={3} bg="blue.800" pt="100px" >
                    <Center>
                        <Box w={1200}  mt='40px'>
                            <Stack w="100%" spacing={0} alignContent='center' color='white' justifyItems={'center'}>
                                <Text textAlign={'center'} fontSize='3xl' fontWeight={'bold'} >21 August 2024</Text>
                                <Text textAlign={'center'} fontSize='5xl' fontWeight={'bold'} style={{lineHeight: 0.9, paddingBottom: '5px'}}>Brazil - Germany</Text>
                                <Text textAlign={'center'} fontSize='xl' fontWeight={'bold'} color='orange.300' pt="20px">starting from $110</Text>
                                <Box>
                                    <Center>
                                        <Button mt='5px' mb='25px' colorScheme='green' size='lg' fontWeight={'bold'} fontSize='2xl' minW='300px' h='60px'>
                                            Grab Your Seats
                                            <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='orange' ml='10px'>
                                                <TagLabel fontSize={'sm'}>25,000 left</TagLabel>
                                            </Tag>                                            
                                        </Button>
                                    </Center>
                                </Box>
                                <Text textAlign={'center'} fontSize='4xl' fontWeight={'bold'} style={{lineHeight: 0.9}}>Football 1/4 Final</Text>
                                <Text textAlign={'center'} fontSize='3xl' >Arena BRB Mane Garrincha</Text>
                            </Stack>
                        </Box>
                    </Center>
                </GridItem>
                <GridItem  h="full"></GridItem>
                <GridItem  h="full" minH={2000}>

                    <Flex mt='40px' wrap='wrap' justify='space-between' gap='40px'>
                        <StatGroup minW='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
                            <Stat color='blue.700'>
                                <StatLabel fontSize='1xl'>21 August 2024</StatLabel>
                                <StatLabel fontSize='1xl' mt="-2px">Arena BRB Mane Garrincha</StatLabel>
                                <StatNumber  fontSize='2xl'>Football 1/4 Final</StatNumber>
                                <StatNumber fontSize='3xl' mt="-12px" fontWeight={800}>Brazil - Germany</StatNumber>
                                <StatNumber fontSize='md' fontWeight='bold' color='orange.600'>starting from $110</StatNumber>
                                <Button mt='2px' colorScheme='green' fontWeight={'bold'} fontSize='xl' h='40px'>
                                    Buy Tickets
                                    <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='orange' ml='10px'>
                                        <TagLabel fontSize={'sm'}>92 left</TagLabel>
                                    </Tag>    
                                </Button>
                            </Stat>
                        </StatGroup>
                        <Box minW='300px' maxW='300px'>
                            <Center>
                                <Heading size='md' mb='20px' >Discover More</Heading>
                            </Center>
                            <form>
                                <SimpleGrid w='full' templateColumns='auto auto' columnGap='2px' rowGap='5px'>
                                    <GridItem>
                                        <FormLabel pt='8px'>Sports</FormLabel>
                                    </GridItem>
                                    <GridItem>
                                        <FormControl>
                                            <Select name='sports' placeholder='All Sports'>
                                                <option value='key1'>Football</option>
                                                <option value='key2'>Basketball</option>
                                            </Select>
                                        </FormControl>
                                    </GridItem>
                                    <GridItem>
                                        <FormLabel pt='8px'>Dates</FormLabel>
                                    </GridItem>
                                    <GridItem>
                                        <FormControl>
                                            <Flex mt='5px' mb='3px' direction='row' justify='space-between' >
                                                <Text>5 August</Text>
                                                <Text>-</Text>
                                                <Text>21 August</Text>
                                            </Flex>
                                            <RangeSlider defaultValue={[1, 17]} min={1} max={17} step={1}>
                                                <RangeSliderTrack bg='red.100'>
                                                    <RangeSliderFilledTrack bg='tomato' />
                                                </RangeSliderTrack>
                                                <RangeSliderThumb boxSize={5} index={0} borderColor='orange.400' />
                                                <RangeSliderThumb boxSize={5} index={1} borderColor='orange.400' />
                                            </RangeSlider>                                            
                                        </FormControl>
                                    </GridItem>
                                </SimpleGrid>
                                <Button width="full" mt={2} type="submit">
                                    Find Events
                                </Button>
                            </form>                            
                        </Box>
                        <StatGroup minW='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
                            <Stat>
                                <StatLabel fontSize='1xl'>21 August 2024</StatLabel>
                                <StatLabel fontSize='1xl' mt="-2px">Arena BRB Mane Garrincha</StatLabel>
                                <StatNumber  fontSize='2xl' color='orange.600' >Football 1/4 Final</StatNumber>
                                <StatNumber fontSize='3xl' mt="-12px" color='orange.600' >Brazil - Germany</StatNumber>
                                <StatNumber fontSize='xl' >$110 - $220</StatNumber>
                                <Button mt='5px' colorScheme='orange' fontWeight={'bold'} fontSize='xl' h='40px'>
                                    Grab Last Seats
                                    <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='green' ml='10px'>
                                        <TagLabel fontSize={'sm'}>100 left</TagLabel>
                                    </Tag>    
                                </Button>
                            </Stat>
                        </StatGroup>
                        <StatGroup minW='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
                            <Stat>
                                <StatLabel fontSize='1xl'>21 August 2024</StatLabel>
                                <StatLabel fontSize='1xl' mt="-2px">Arena BRB Mane Garrincha</StatLabel>
                                <StatNumber  fontSize='2xl' color='orange.600' >Football 1/4 Final</StatNumber>
                                <StatNumber fontSize='3xl' mt="-12px" color='orange.600' >Brazil - Germany</StatNumber>
                                <StatNumber fontSize='xl' >$110 - $220</StatNumber>
                                <Button mt='5px' colorScheme='green' fontWeight={'bold'} fontSize='xl' h='40px'>
                                    Grab Seats
                                    <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='orange' ml='10px'>
                                        <TagLabel fontSize={'sm'}>92 left</TagLabel>
                                    </Tag>    
                                </Button>
                            </Stat>
                        </StatGroup>
                        <StatGroup minW='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
                            <Stat>
                                <StatLabel fontSize='1xl'>21 August 2024</StatLabel>
                                <StatLabel fontSize='1xl' mt="-2px">Arena BRB Mane Garrincha</StatLabel>
                                <StatNumber  fontSize='2xl' color='orange.600' >Football 1/4 Final</StatNumber>
                                <StatNumber fontSize='3xl' mt="-12px" color='orange.600' >Brazil - Germany</StatNumber>
                                <StatNumber fontSize='xl' >$110 - $220</StatNumber>
                                <Button mt='5px' colorScheme='green' fontWeight={'bold'} fontSize='xl' h='40px'>
                                    Grab Seats
                                    <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='orange' ml='10px'>
                                        <TagLabel fontSize={'sm'}>92 left</TagLabel>
                                    </Tag>    
                                </Button>
                            </Stat>
                        </StatGroup>
                        <StatGroup minW='370px' border={'1px solid lightgray'} p={4} borderRadius={'xl'}>
                            <Stat>
                                <StatLabel fontSize='1xl'>21 August 2024</StatLabel>
                                <StatLabel fontSize='1xl' mt="-2px">Arena BRB Mane Garrincha</StatLabel>
                                <StatNumber  fontSize='2xl' color='orange.600' >Football 1/4 Final</StatNumber>
                                <StatNumber fontSize='3xl' mt="-12px" color='orange.600' >Brazil - Germany</StatNumber>
                                <StatNumber fontSize='xl' >$110 - $220</StatNumber>
                                <Button mt='5px' colorScheme='green' fontWeight={'bold'} fontSize='xl' h='40px'>
                                    Grab Seats
                                    <Tag size={'sm'} borderRadius='full' variant='solid' colorScheme='orange' ml='10px'>
                                        <TagLabel fontSize={'sm'}>92 left</TagLabel>
                                    </Tag>    
                                </Button>
                            </Stat>
                        </StatGroup>
                    </Flex>
                    
                </GridItem>
                <GridItem  h="full"></GridItem>
            </Grid>  
        </>
    )
}

export default IndexPage

export async function getServerSideProps(): Promise<{ props: IndexPageProps }> {
    try {
        const res = await fetch(`http://localhost:3001/search?selling=true&count=9`)
        const envelope = await res.json()
        const events = envelope.data as EventSearchResult[]
        return { 
            props: { 
                events 
            }
        }
    } catch(err) {
        console.log(err)
        return { 
            props: {
                events: []
            }
        }
    }
}
